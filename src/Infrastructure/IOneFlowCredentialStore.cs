namespace OneFlowApis.Infrastructure;

public interface IOneFlowCredentialStore
{
    void PersistRefreshedCredentials(string token, string refreshToken);

    OneFlowCredentialStoreDiagnostics GetDiagnostics();
}
