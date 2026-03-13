using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using OneFlowApis.Endpoints;
using OneFlowApis.Infrastructure;
using OneFlowApis.Middlewares;
using OneFlowApis.Models;
using OneFlowApis.Security;
using OneFlowApis.Services;
using Scalar.AspNetCore;

DotEnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);
var oneFlowOptions = OneFlowOptions.FromConfiguration(builder.Configuration);
var internalApiSecurityOptions = InternalApiSecurityOptions.FromConfiguration(builder.Configuration);
var resilienceOptions = OneFlowResilienceOptions.FromConfiguration(builder.Configuration);

builder.WebHost.UseUrls($"http://0.0.0.0:{oneFlowOptions.Port}");

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddSingleton(oneFlowOptions);
builder.Services.AddSingleton(internalApiSecurityOptions);
builder.Services.AddSingleton(resilienceOptions);
builder.Services.AddHttpClient("OneFlowUpstream", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient("OmiePortal", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services
    .AddAuthentication(InternalApiSecurityOptions.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, InternalApiKeyAuthenticationHandler>(
        InternalApiSecurityOptions.AuthenticationScheme,
        _ => { });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "One-Flow-APIs",
        Version = "v1",
        Description = "API interna em ASP.NET Core para integracao com o OneFlow."
    });

    options.AddSecurityDefinition(InternalApiSecurityOptions.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = $"Informe a chave interna no header {internalApiSecurityOptions.HeaderName}.",
        Name = internalApiSecurityOptions.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = InternalApiSecurityOptions.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            }
        ] = Array.Empty<string>()
    });
});
builder.Services.AddSingleton<OneFlowResiliencePolicy>();
builder.Services.AddSingleton<OneFlowAuthManager>();
builder.Services.AddSingleton<OneFlowClient>();
builder.Services.AddSingleton<OneFlowService>();

var app = builder.Build();

if (internalApiSecurityOptions.IsConfigured)
{
    app.Logger.LogInformation(
        "Autenticacao interna habilitada com header {HeaderName}.",
        internalApiSecurityOptions.HeaderName);
}
else
{
    app.Logger.LogWarning(
        "Autenticacao interna desabilitada. Preencha INTERNAL_API_KEY para proteger os endpoints internos.");
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "docs";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "One-Flow-APIs v1");
    options.DisplayRequestDuration();
});
app.MapScalarApiReference("/scalar", options =>
{
    options.WithTitle("One-Flow-APIs")
        .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
        .DisableDefaultFonts();
});

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();

var api = app.MapGroup("/api/oneflow").RequireAuthorization();

api.MapDiagnosticEndpoints();
api.MapEmpresaEndpoints();
api.MapEscritorioEndpoints();
api.MapFiscalEndpoints();
api.MapFolhaEndpoints();
api.MapContabilidadeEndpoints();
api.MapObrigacoesEndpoints();

app.Run();

public partial class Program;
