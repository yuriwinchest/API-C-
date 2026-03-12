[CmdletBinding()]
param(
    [switch]$UseRunningApi,
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

$script:Results = [System.Collections.Generic.List[object]]::new()
$repoRoot = Split-Path -Parent $PSScriptRoot
$envPath = Join-Path $repoRoot ".env"
$logDirectory = Join-Path $repoRoot "TestResults\smoke"
$stdoutLog = Join-Path $logDirectory "final-smoke.stdout.log"
$stderrLog = Join-Path $logDirectory "final-smoke.stderr.log"

function Read-DotEnv {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "Arquivo .env nao encontrado em $Path."
    }

    $values = @{}

    foreach ($rawLine in Get-Content $Path) {
        $line = $rawLine.Trim()

        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            continue
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -le 0) {
            continue
        }

        $key = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim().Trim('"')
        $values[$key] = $value
    }

    return $values
}

function Test-IsConfiguredValue {
    param(
        [string]$Value
    )

    return -not [string]::IsNullOrWhiteSpace($Value) -and
        -not $Value.StartsWith("preencher_", [System.StringComparison]::OrdinalIgnoreCase)
}

function Add-CheckResult {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [bool]$Success,
        [Parameter(Mandatory = $true)]
        [string]$Details
    )

    $script:Results.Add([pscustomobject]@{
            Check   = $Name
            Success = $Success
            Details = $Details
        })
}

function Invoke-SmokeRequest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Uri,
        [hashtable]$Headers
    )

    return Invoke-WebRequest `
        -Uri $Uri `
        -Headers $Headers `
        -Method Get `
        -SkipHttpErrorCheck `
        -TimeoutSec 30
}

function Convert-JsonBody {
    param(
        [string]$Content
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $null
    }

    return $Content | ConvertFrom-Json -Depth 20
}

function Wait-ForApi {
    param(
        [Parameter(Mandatory = $true)]
        [string]$HealthUrl,
        [Parameter(Mandatory = $true)]
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $HealthUrl -Method Get -TimeoutSec 5 -SkipHttpErrorCheck
            if ([int]$response.StatusCode -eq 200) {
                return
            }
        }
        catch {
        }

        Start-Sleep -Seconds 1
    }

    throw "A API nao respondeu em $HealthUrl dentro de $TimeoutSeconds segundos."
}

function Get-LogTail {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        return ""
    }

    return (Get-Content $Path -Tail 30) -join [Environment]::NewLine
}

$process = $null

try {
    $envValues = Read-DotEnv -Path $envPath
    $port = 3000

    if ($envValues.ContainsKey("PORT") -and [int]::TryParse($envValues["PORT"], [ref]$port) -and $port -gt 0) {
        $port = [int]$envValues["PORT"]
    }

    $baseUrl = "http://127.0.0.1:$port"
    $healthUrl = "$baseUrl/health"
    $swaggerUrl = "$baseUrl/swagger/v1/swagger.json"
    $configUrl = "$baseUrl/api/oneflow/configuracao/status"
    $obrigacoesUrl = "$baseUrl/api/oneflow/guias/obrigacoes/geral"
    $headerName = if ($envValues["INTERNAL_API_KEY_HEADER_NAME"]) { $envValues["INTERNAL_API_KEY_HEADER_NAME"] } else { "X-Internal-Api-Key" }
    $internalApiKey = $envValues["INTERNAL_API_KEY"]
    $internalApiConfigured = Test-IsConfiguredValue -Value $internalApiKey
    $expectsRefresh = (Test-IsConfiguredValue -Value $envValues["ONEFLOW_COMPANY_REFRESH_TOKEN"]) -and
        (Test-IsConfiguredValue -Value $envValues["ONEFLOW_COMPANY_APP_HASH"])

    if (-not $UseRunningApi) {
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
        Remove-Item $stdoutLog, $stderrLog -ErrorAction SilentlyContinue

        $process = Start-Process `
            -FilePath "dotnet" `
            -ArgumentList @("run", "--project", ".\OneFlowApis.csproj") `
            -WorkingDirectory $repoRoot `
            -PassThru `
            -RedirectStandardOutput $stdoutLog `
            -RedirectStandardError $stderrLog

        Wait-ForApi -HealthUrl $healthUrl -TimeoutSeconds $StartupTimeoutSeconds
    }

    $healthResponse = Invoke-SmokeRequest -Uri $healthUrl
    $healthPayload = Convert-JsonBody -Content $healthResponse.Content
    $healthOk = ([int]$healthResponse.StatusCode -eq 200) -and $healthPayload -and $healthPayload.status -eq "ok"
    Add-CheckResult -Name "health" -Success $healthOk -Details "status=$([int]$healthResponse.StatusCode)"

    $swaggerResponse = Invoke-SmokeRequest -Uri $swaggerUrl
    $swaggerOk = [int]$swaggerResponse.StatusCode -eq 200
    Add-CheckResult -Name "swagger" -Success $swaggerOk -Details "status=$([int]$swaggerResponse.StatusCode)"

    if ($internalApiConfigured) {
        $unauthorizedResponse = Invoke-SmokeRequest -Uri $configUrl
        $unauthorizedOk = [int]$unauthorizedResponse.StatusCode -eq 401
        Add-CheckResult -Name "auth-interna-sem-header" -Success $unauthorizedOk -Details "status=$([int]$unauthorizedResponse.StatusCode)"
    }

    $headers = @{}
    if ($internalApiConfigured) {
        $headers[$headerName] = $internalApiKey
    }

    $configResponse = Invoke-SmokeRequest -Uri $configUrl -Headers $headers
    $configPayload = Convert-JsonBody -Content $configResponse.Content
    $configOk = [int]$configResponse.StatusCode -eq 200 -and
        $configPayload.prontoParaTesteOneFlow -eq $true -and
        ($configPayload.prontoParaRenovacaoAutomatica -eq $expectsRefresh) -and
        ($configPayload.prontoParaAutenticacaoInterna -eq $internalApiConfigured)

    Add-CheckResult `
        -Name "configuracao-status" `
        -Success $configOk `
        -Details "status=$([int]$configResponse.StatusCode); prontoParaTesteOneFlow=$($configPayload.prontoParaTesteOneFlow); prontoParaRenovacaoAutomatica=$($configPayload.prontoParaRenovacaoAutomatica); prontoParaAutenticacaoInterna=$($configPayload.prontoParaAutenticacaoInterna)"

    $obrigacoesResponse = Invoke-SmokeRequest -Uri $obrigacoesUrl -Headers $headers
    $obrigacoesPayload = Convert-JsonBody -Content $obrigacoesResponse.Content
    $obrigacoesCount = 0
    if ($obrigacoesPayload -and $obrigacoesPayload.result -and $obrigacoesPayload.result.obrigacoes) {
        $obrigacoesCount = @($obrigacoesPayload.result.obrigacoes).Count
    }

    $obrigacoesOk = [int]$obrigacoesResponse.StatusCode -eq 200 -and $obrigacoesCount -gt 0
    Add-CheckResult -Name "oneflow-obrigacoes-geral" -Success $obrigacoesOk -Details "status=$([int]$obrigacoesResponse.StatusCode); obrigacoes=$obrigacoesCount"

    $script:Results | Format-Table -AutoSize

    $failedChecks = $script:Results | Where-Object { -not $_.Success }
    if ($failedChecks) {
        throw "Smoke check final falhou. Revise a tabela acima."
    }

    Write-Host ""
    Write-Host "Smoke check final concluido com sucesso."
    if (-not $UseRunningApi) {
        Write-Host "Logs da execucao: $logDirectory"
    }
}
catch {
    $script:Results | Format-Table -AutoSize

    if ($process -and -not $process.HasExited) {
        Write-Host ""
        Write-Host "Ultimas linhas de stdout:"
        Write-Host (Get-LogTail -Path $stdoutLog)
        Write-Host ""
        Write-Host "Ultimas linhas de stderr:"
        Write-Host (Get-LogTail -Path $stderrLog)
    }

    throw
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
