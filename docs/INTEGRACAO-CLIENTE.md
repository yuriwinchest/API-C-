# Integracao do Cliente

## Objetivo

Este documento orienta a integracao da API com o sistema externo do cliente sem versionar segredos no GitHub.

## O que o cliente recebe

- codigo-fonte pelo GitHub
- arquivo `.env` por canal privado

## O que o cliente deve fazer

1. Clonar ou baixar o repositorio.
2. Colocar o arquivo `.env` recebido na raiz do projeto.
3. Confirmar que o `.env` nao sera commitado nem compartilhado em repositorios.
4. Restaurar e compilar o projeto:

```powershell
dotnet restore
dotnet build
```

5. Executar a validacao local:

```powershell
pwsh -File .\scripts\FinalSmokeTest.ps1
```

6. Subir a API:

```powershell
dotnet run --project .\OneFlowApis.csproj
```

## Como integrar com a aplicacao principal

Fluxo recomendado:

1. frontend da aplicacao principal
2. backend ou BFF da aplicacao principal
3. API One-Flow-APIs
4. OneFlow

## Regra de seguranca

- nao expor `INTERNAL_API_KEY` no frontend
- nao salvar `INTERNAL_API_KEY` em codigo JavaScript entregue ao navegador
- usar a chave apenas no backend ou proxy seguro

## Endpoints iniciais recomendados para homologacao

- `GET /api/oneflow/configuracao/status`
- `GET /api/oneflow/guias/obrigacoes/geral`

## Evidencia minima esperada antes da homologacao

- `GET /health` retorna `200`
- `GET /swagger/v1/swagger.json` retorna `200`
- endpoint interno sem header retorna `401`
- `GET /api/oneflow/configuracao/status` retorna que o ambiente esta pronto para teste
- `GET /api/oneflow/guias/obrigacoes/geral` retorna dados reais do OneFlow

## Observacao final

Se o frontend do cliente estiver em outro projeto, a integracao deve ser feita pelo backend desse projeto. Esta API nao foi preparada para expor a chave interna no navegador.
