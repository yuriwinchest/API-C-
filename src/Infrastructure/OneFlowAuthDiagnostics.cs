namespace OneFlowApis.Infrastructure;

public sealed record OneFlowAuthDiagnostics(
    OneFlowTokenDiagnostics Token,
    OneFlowRefreshDiagnostics RenovacaoAutomatica,
    OneFlowCredentialStoreDiagnostics Persistencia);

public sealed record OneFlowTokenDiagnostics(
    bool Configurado,
    string? Identificador,
    bool FormatoJwtReconhecido,
    bool Expirado,
    DateTimeOffset? ExpiraEmUtc,
    long? SegundosRestantes);

public sealed record OneFlowRefreshDiagnostics(
    bool Habilitada,
    IReadOnlyList<string> ConfiguracoesAusentes,
    string? UltimoGatilho,
    string? UltimoResultado,
    DateTimeOffset? UltimaTentativaEmUtc,
    DateTimeOffset? UltimoSucessoEmUtc,
    DateTimeOffset? UltimaFalhaEmUtc,
    string? UltimaFalhaMensagem);

public sealed record OneFlowCredentialStoreDiagnostics(
    string ModoPersistencia,
    bool ArquivoEnvEncontrado,
    bool ArquivoEnvGravavel,
    bool? UltimaPersistenciaBemSucedida,
    DateTimeOffset? UltimaPersistenciaEmUtc,
    string? UltimaPersistenciaMensagem);
