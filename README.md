# One-Flow-APIs

API em C# com ASP.NET Core para integrar os endpoints do OneFlow necessarios para folha, contabilidade e guias/obrigacoes.

## Objetivo

Este projeto expoe uma API interna organizada e validada, que faz proxy para os endpoints oficiais do OneFlow. O token JWT da empresa fica isolado em variaveis de ambiente e nao deve ser versionado.

## Stack

- .NET 8
- ASP.NET Core Minimal API
- xUnit

## Configuracao

1. Copie `.env.example` para `.env`.
2. Preencha as variaveis de autenticacao do OneFlow.
3. Execute os comandos:

```bash
dotnet restore
dotnet build
```

## Variaveis de ambiente

| Variavel | Obrigatoria | Descricao |
| --- | --- | --- |
| `PORT` | Nao | Porta local da API. Padrao: `3000`. |
| `ONEFLOW_BASE_URL` | Nao | Base da API oficial. Padrao: `https://rest.oneflow.com.br/api`. |
| `OMIE_PORTAL_APPS_BASE_URL` | Nao | Base do portal Omie usada na renovacao do token. |
| `ONEFLOW_COMPANY_TOKEN` | Sim | Token JWT da empresa usado no header `Authorization: Bearer <token>`. |
| `ONEFLOW_COMPANY_REFRESH_TOKEN` | Recomendado | Refresh token da empresa para renovacao automatica. |
| `ONEFLOW_COMPANY_APP_HASH` | Recomendado | `app_hash` da empresa no OneFlow, necessario para renovar o token. |

## Execucao

Desenvolvimento:

```bash
dotnet run --project .\OneFlowApis.csproj
```

Build:

```bash
dotnet build
```

Testes:

```bash
dotnet test .\tests\OneFlowApis.Tests\OneFlowApis.Tests.csproj
```

## Healthcheck

```http
GET /health
```

Resposta esperada:

```json
{
  "status": "ok",
  "servico": "one-flow-apis"
}
```

## Endpoints internos

### Folha de pagamento e trabalhador

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/folha/variaveis` | `/oneflow/empresa/folha/variaveis/incluir` |
| `GET` | `/api/oneflow/folha/trabalhadores/dados-basicos` | `/oneflow/empresa/folha/trabalhador/dadosbasicos` |
| `GET` | `/api/oneflow/folha/trabalhadores/eventos` | `/oneflow/empresa/folha/trabalhador/eventos` |
| `GET` | `/api/oneflow/folha/holerites/totais` | `/oneflow/empresa/folha/recibos/totais` |
| `GET` | `/api/oneflow/folha/datas` | `/oneflow/empresa/folha/datas` |
| `GET` | `/api/oneflow/folha/status` | `/oneflow/empresa/folha/statusfolha` |
| `GET` | `/api/oneflow/folha/fator-r` | `/oneflow/empresa/folha/fatorr` |

### Fiscal

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/fiscal/nfse/nacional` | `/oneflow/empresa/fiscal/nfse/nacional` |
| `POST` | `/api/oneflow/fiscal/nfse/prefeitura` | `/oneflow/empresa/fiscal/nfse/prefeitura` |
| `POST` | `/api/oneflow/fiscal/documentos/remover` | `/oneflow/empresa/fiscal/documentos/remover` |
| `POST` | `/api/oneflow/fiscal/documentos/alterar-status` | `/oneflow/empresa/fiscal/documentos/alterarstatus` |
| `GET` | `/api/oneflow/fiscal/documentos/totais` | `/oneflow/empresa/fiscal/documentos/totais` |
| `GET` | `/api/oneflow/fiscal/documentos/quantidade` | `/oneflow/empresa/fiscal/documentos/quantidade` |
| `GET` | `/api/oneflow/fiscal/documentos/listar` | `/oneflow/empresa/fiscal/documentos/listar` |
| `GET` | `/api/oneflow/fiscal/apuracoes` | `/oneflow/empresa/fiscal/apuracao/listar` |
| `GET` | `/api/oneflow/fiscal/simples-nacional/aliquotas` | `/oneflow/empresa/fiscal/simplesnacional/aliquotas` |

### Contabilidade

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/contabilidade/plano-contas` | `/oneflow/empresa/contabil/planocontas/contas` |
| `POST` | `/api/oneflow/contabilidade/lancamentos` | `/oneflow/empresa/contabil/lancamentos/gerarlancamento` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/transacao` | `/oneflow/empresa/contabil/lancamentos/gerartransacao` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/padrao` | `/oneflow/empresa/contabil/lancamentos/gerarpadrao` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir` | `/oneflow/empresa/contabil/lancamentos/excluirlancamento` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir-transacao` | `/oneflow/empresa/contabil/lancamentos/excluirtransacao` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir-padrao` | `/oneflow/empresa/contabil/lancamentos/excluirpadrao` |
| `GET` | `/api/oneflow/contabilidade/balancete` | `/oneflow/empresa/contabil/balancete` |
| `GET` | `/api/oneflow/contabilidade/razao` | `/oneflow/empresa/contabil/razao` |

### Guias e obrigacoes

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/guias/anexos` | `/oneflow/empresa/obrigacoes/anexos` |

## Parametros validados

- `competencia`, `competenciaInicial` e `competenciaFinal`: formato `AAAAMM`
- `cpf` ou `matricula`: pelo menos um dos dois deve ser informado nos endpoints de trabalhador
- `tipoFolha`: inteiro positivo
- `zeramento`: `S` ou `N`
- `codigo`: codigo da obrigacao no OneFlow
- `id` ou `pagina`: pelo menos um dos dois deve ser informado em `plano-contas`

## Compatibilidade de contrato

Para reduzir atrito com a documentacao operacional do cliente, a API expoe dois estilos de rota:

- rotas padronizadas em kebab-case, como `/api/oneflow/fiscal/documentos/listar`
- aliases compativeis com a nomenclatura legada dos materiais, como `/api/oneflow/fiscal/documentos/total/competencia`

Os aliases foram adicionados para:

- fiscal: `nfsenacional/incluir`, `nfseprefeitura/incluir`, `nfse/remove`, `nfsestatus/alterar`, `total/competencia`, `documentos/total/competencia`, `apuracao/competencia/imposto`, `aliquotas/simplesnacional/competencia`
- folha: `variaveisfolha`, `dadosbasicostrabalhador/competencia`, `eventosdostrabalhadores/competencia`, `statusfolha/competencia`, `datasconfigurada/competencia`, `fatorr/competencia`, `recibos/totais/competencia`
- contabilidade: `planocontas`, `lancamentos/conta`

## Observacoes importantes

- O repositorio nao versiona segredos.
- `App Key` e `App Secret` do Omie nao substituem o `ONEFLOW_COMPANY_TOKEN`.
- A API ficou preparada para renovar automaticamente o token da empresa quando `ONEFLOW_COMPANY_REFRESH_TOKEN` e `ONEFLOW_COMPANY_APP_HASH` forem informados.
- Os payloads `POST` foram mantidos flexiveis para receber o JSON final conforme a modelagem oficial do OneFlow.

## Referencias oficiais

- Autenticacao OneFlow via Omie: [ajuda.omie.com.br](https://ajuda.omie.com.br/pt-BR/articles/9113296-autenticacao-para-utilizacao-de-apis-do-oneflow)
- Especificacao SwaggerHub OneFlow 2.0.0: [api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0](https://api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0)
