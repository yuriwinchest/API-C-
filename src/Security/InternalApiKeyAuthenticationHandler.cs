using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OneFlowApis.Models;

namespace OneFlowApis.Security;

public sealed class InternalApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly InternalApiSecurityOptions _securityOptions;

    public InternalApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        InternalApiSecurityOptions securityOptions)
        : base(options, logger, encoder)
    {
        _securityOptions = securityOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_securityOptions.IsConfigured)
        {
            return Task.FromResult(CreateSuccessResult("autenticacao-interna-desabilitada"));
        }

        if (!Request.Headers.TryGetValue(_securityOptions.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Cabecalho {_securityOptions.HeaderName} nao informado."));
        }

        var providedApiKey = headerValues.ToString().Trim();
        var expectedApiKey = _securityOptions.ApiKey ?? string.Empty;

        if (!FixedTimeEquals(providedApiKey, expectedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Chave interna invalida."));
        }

        return Task.FromResult(CreateSuccessResult("integracao-interna"));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/json";

        return Response.WriteAsJsonAsync(new
        {
            mensagem = "Acesso nao autorizado.",
            detalhes = new
            {
                cabecalho = _securityOptions.HeaderName
            }
        });
    }

    private AuthenticateResult CreateSuccessResult(string identityName)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, identityName)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static bool FixedTimeEquals(string providedValue, string expectedValue)
    {
        var providedBytes = Encoding.UTF8.GetBytes(providedValue);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedValue);

        if (providedBytes.Length != expectedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
