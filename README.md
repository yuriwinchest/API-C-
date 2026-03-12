# One-Flow-APIs

API em C# com ASP.NET Core para integrar os endpoints do OneFlow necessarios para folha, contabilidade e guias/obrigacoes.

## Objetivo

Este projeto expoe uma API interna organizada e validada, que faz proxy para os endpoints oficiais do OneFlow. O token JWT da empresa fica isolado em variaveis de ambiente e nao deve ser versionado.

## Stack

- .NET 8
- ASP.NET Core Minimal API
- Swagger / OpenAPI
- xUnit

## Configuracao

1. Copie `.env.example` para `.env`.
2. Preencha as variaveis de autenticacao interna e do OneFlow.
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
| `OMIE_APP_KEY` | Futuro uso | Campo reservado para a `App Key` do Omie. |
| `OMIE_APP_SECRET` | Futuro uso | Campo reservado para o `App Secret` do Omie. |
| `INTERNAL_API_KEY_HEADER_NAME` | Nao | Header usado para proteger a API interna. Padrao: `X-Internal-Api-Key`. |
| `INTERNAL_API_KEY` | Recomendado | Chave da autenticacao interna da API. Enquanto nao for preenchida, a protecao fica desabilitada. |
| `ONEFLOW_COMPANY_TOKEN` | Sim | Token JWT da empresa usado no header `Authorization: Bearer <token>`. |
| `ONEFLOW_COMPANY_REFRESH_TOKEN` | Recomendado | Refresh token da empresa para renovacao automatica. |
| `ONEFLOW_COMPANY_APP_HASH` | Recomendado | `app_hash` da empresa no OneFlow, necessario para renovar o token. |
| `ONEFLOW_HTTP_TIMEOUT_SECONDS` | Nao | Timeout por tentativa para chamadas ao OneFlow. Padrao: `30`. |
| `ONEFLOW_HTTP_RETRY_COUNT` | Nao | Quantidade de tentativas extras para chamadas idempotentes (`GET`). Padrao: `2`. |
| `ONEFLOW_HTTP_RETRY_BASE_DELAY_MS` | Nao | Delay base entre tentativas em milissegundos. Padrao: `500`. |
| `ONEFLOW_HTTP_CIRCUIT_BREAKER_FAILURE_THRESHOLD` | Nao | Numero de falhas consecutivas para abrir o circuit breaker. Padrao: `5`. |
| `ONEFLOW_HTTP_CIRCUIT_BREAKER_BREAK_SECONDS` | Nao | Tempo em segundos com o circuito aberto. Padrao: `30`. |
| `GCLICK_BASE_URL` | Nao | URL base reservada para a integracao com o G-Click. |
| `GCLICK_CLIENT_ID` | Futuro uso | Campo reservado para o `Client ID` do G-Click. |
| `GCLICK_CLIENT_SECRET` | Futuro uso | Campo reservado para o `Client Secret` do G-Click. |

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

## Como rodar localmente e abrir a interface grafica

1. Entre na pasta do projeto.
2. Execute:

```bash
dotnet run --project .\OneFlowApis.csproj
```

3. Quando a API subir, abra no navegador:

- Swagger UI: [http://localhost:3000/docs](http://localhost:3000/docs)
- Scalar API Reference: [http://localhost:3000/scalar](http://localhost:3000/scalar)
- OpenAPI JSON: [http://localhost:3000/swagger/v1/swagger.json](http://localhost:3000/swagger/v1/swagger.json)
- Healthcheck: [http://localhost:3000/health](http://localhost:3000/health)

Observacoes:

- Se a porta no `.env` for diferente de `3000`, substitua a porta nas URLs acima.
- Se `INTERNAL_API_KEY` estiver preenchida no `.env`, os endpoints `/api/oneflow/...` exigirao o header `X-Internal-Api-Key`.
- O Swagger e o Scalar servem para explorar e testar os endpoints visualmente no ambiente local.

## Smoke check final local

Para validar rapidamente o ambiente local ja configurado, execute:

```powershell
pwsh -File .\scripts\FinalSmokeTest.ps1
```

Esse script:

- le o `.env`
- sobe a API localmente
- valida `health`, `swagger`, autenticacao interna e status de configuracao
- executa uma chamada real em `/api/oneflow/guias/obrigacoes/geral`
- encerra o processo ao final

Se a API ja estiver rodando e voce quiser reaproveitar a instancia atual:

```powershell
pwsh -File .\scripts\FinalSmokeTest.ps1 -UseRunningApi
```

## Entrega para o cliente

Para entregar esta API ao cliente sem expor segredos no GitHub:

1. O cliente baixa o projeto normalmente do repositorio.
2. O arquivo de credenciais deve ser enviado fora do GitHub, por canal privado.
3. O cliente copia o arquivo recebido para a raiz do projeto com o nome `.env`.
4. O cliente executa o smoke check final para confirmar que o ambiente local ficou valido.

Observacoes importantes:

- o repositorio deve continuar sem segredos versionados
- `.env` e `original.env` nao devem ser enviados para o GitHub
- o arquivo enviado por WhatsApp serve apenas para uso local ou ambiente privado controlado
- antes de homologar, o cliente deve rodar `pwsh -File .\scripts\FinalSmokeTest.ps1`

## Integracao com sistema externo

Esta API foi desenhada para ser consumida por outro sistema. O fluxo recomendado e:

1. frontend da aplicacao principal
2. backend ou camada de integracao da aplicacao principal
3. esta API
4. OneFlow

Regra de seguranca:

- a `INTERNAL_API_KEY` nao deve ficar exposta no navegador
- o frontend nao deve chamar esta API diretamente se isso exigir expor a chave interna
- a chave interna deve ser usada apenas no backend, proxy ou BFF da aplicacao principal

Checklist minimo para o cliente integrar:

1. baixar o projeto
2. receber o arquivo `.env` por canal privado
3. posicionar o `.env` na raiz
4. executar `dotnet restore`
5. executar `dotnet build`
6. executar `pwsh -File .\scripts\FinalSmokeTest.ps1`
7. subir a API com `dotnet run --project .\OneFlowApis.csproj`
8. integrar o backend da aplicacao principal nos endpoints `/api/oneflow/...`

## Acesso e documentacao interativa

- Swagger UI: `/docs`
- Scalar API Reference: `/scalar`
- OpenAPI JSON: `/swagger/v1/swagger.json`
- Header da API interna: `X-Internal-Api-Key: SUA_CHAVE`

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

## Diagnostico de configuracao

```http
GET /api/oneflow/configuracao/status
```

Esse endpoint nao expoe segredos. Ele informa apenas se os campos obrigatorios e opcionais ja foram preenchidos para OneFlow, Omie e G-Click.

## Seguranca aplicada

- Autenticacao interna por header configuravel.
- Segredos mantidos apenas em variaveis de ambiente.
- Comparacao da chave interna em tempo constante.
- Middleware centralizado de tratamento de erros.
- Endpoint de diagnostico sem exposicao de segredos.

## Resiliencia e escalabilidade

- Timeout por tentativa para chamadas ao upstream.
- Retry apenas para chamadas idempotentes (`GET`, `HEAD`, `OPTIONS`) para evitar duplicidade em operacoes de escrita.
- Circuit breaker para reduzir cascata de falhas quando o OneFlow estiver indisponivel.
- Logging estruturado em JSON com correlation id no header `X-Correlation-ID`.
- Rotas modularizadas por dominio: fiscal, folha, contabilidade, obrigacoes e diagnostico.

## Endpoints internos

### Folha de pagamento e trabalhador

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/folha/variaveis` | `/oneflow/empresa/folha/variaveis/incluir` |
| `GET` | `/api/oneflow/folha/trabalhadores/dados-basicos` | `/oneflow/empresa/folha/trabalhador/dadosbasicos` |
| `GET` | `/api/oneflow/folha/trabalhadores/eventos` | `/oneflow/empresa/folha/trabalhador/eventos` |
| `GET` | `/api/oneflow/folha/rubricas/dados-basicos` | `/oneflow/empresa/folha/rubrica/dadosbasicos` |
| `GET` | `/api/oneflow/folha/holerites/totais` | `/oneflow/empresa/folha/recibos/totais` |
| `GET` | `/api/oneflow/folha/datas` | `/oneflow/empresa/folha/datas` |
| `GET` | `/api/oneflow/folha/status` | `/oneflow/empresa/folha/statusfolha` |
| `GET` | `/api/oneflow/folha/fator-r` | `/oneflow/empresa/folha/fatorr` |

### Fiscal

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/fiscal/nfse/nacional` | `/oneflow/empresa/fiscal/nfse/nacional` |
| `POST` | `/api/oneflow/fiscal/nfse/prefeitura` | `/oneflow/empresa/fiscal/nfse/prefeitura` |
| `POST` | `/api/oneflow/fiscal/nfse/layout-oneflow` | `/oneflow/empresa/fiscal/nfse/layoutoneflow` |
| `POST` | `/api/oneflow/fiscal/documentos/remover` | `/oneflow/empresa/fiscal/documentos/remover` |
| `POST` | `/api/oneflow/fiscal/documentos/alterar-status` | `/oneflow/empresa/fiscal/documentos/alterarstatus` |
| `GET` | `/api/oneflow/fiscal/documentos/totais` | `/oneflow/empresa/fiscal/documentos/totais` |
| `GET` | `/api/oneflow/fiscal/documentos/quantidade` | `/oneflow/empresa/fiscal/documentos/quantidade` |
| `GET` | `/api/oneflow/fiscal/documentos/listar` | `/oneflow/empresa/fiscal/documentos/listar` |
| `GET` | `/api/oneflow/fiscal/documentos/por-socio` | `/oneflow/empresa/fiscal/documentos/porsocio` |
| `GET` | `/api/oneflow/fiscal/apuracoes` | `/oneflow/empresa/fiscal/apuracao/listar` |
| `GET` | `/api/oneflow/fiscal/apuracoes/resumo` | `/oneflow/empresa/fiscal/apuracao/resumo` |
| `GET` | `/api/oneflow/fiscal/simples-nacional/aliquotas` | `/oneflow/empresa/fiscal/simplesnacional/aliquotas` |

### Contabilidade

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/contabilidade/plano-contas` | `/oneflow/empresa/contabil/planocontas/contas` |
| `POST` | `/api/oneflow/contabilidade/lancamentos` | `/oneflow/empresa/contabil/lancamentos/gerarlancamento` |
| `POST` | `/api/oneflow/contabilidade/lancamentoscontabeis` | Alias legado para `gerarlancamento` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/transacao` | `/oneflow/empresa/contabil/lancamentos/gerartransacao` |
| `POST` | `/api/oneflow/contabilidade/lancamentoscontabeis/transacao` | Alias legado para `gerartransacao` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/padrao` | `/oneflow/empresa/contabil/lancamentos/gerarpadrao` |
| `POST` | `/api/oneflow/contabilidade/lancamentoscontabeis/padrao` | Alias legado para `gerarpadrao` |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir` | `/oneflow/empresa/contabil/lancamentos/excluirlancamento` |
| `DELETE` | `/api/oneflow/contabilidade/lancamentoscontabeis/id` | Alias legado para exclusao por lancamento |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir-transacao` | `/oneflow/empresa/contabil/lancamentos/excluirtransacao` |
| `DELETE` | `/api/oneflow/contabilidade/lancamentoscontabeis/transacao` | Alias legado para exclusao por transacao |
| `POST` | `/api/oneflow/contabilidade/lancamentos/excluir-padrao` | `/oneflow/empresa/contabil/lancamentos/excluirpadrao` |
| `DELETE` | `/api/oneflow/contabilidade/lancamentoscontabeis/padrao` | Alias legado para exclusao por padrao |
| `GET` | `/api/oneflow/contabilidade/balancete` | `/oneflow/empresa/contabil/balancete` |
| `GET` | `/api/oneflow/contabilidade/razao` | `/oneflow/empresa/contabil/razao` |

### Guias e obrigacoes

| Metodo | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/guias/anexos` | `/oneflow/empresa/obrigacoes/anexos` |
| `GET` | `/api/oneflow/guias/obrigacoes/geral` | `/oneflow/empresa/obrigacoes/geral` |
| `GET` | `/api/oneflow/guias/obrigacoes/listar` | `/oneflow/empresa/obrigacoes/listar` |
| `POST` | `/api/oneflow/guias/obrigacoes/incluir` | `/oneflow/empresa/obrigacoes/incluir` |
| `GET` | `/api/oneflow/obrigacao/anexo` | `/oneflow/empresa/obrigacoes/anexos` |

## Parametros validados

- `competencia`, `competenciaInicial` e `competenciaFinal`: formato `AAAAMM`
- `cpf` ou `matricula`: pelo menos um dos dois deve ser informado nos endpoints de trabalhador
- `tipoFolha`: inteiro positivo
- `zeramento`: `S` ou `N`
- `codigo`: codigo da obrigacao no OneFlow
- `id` ou `pagina`: pelo menos um dos dois deve ser informado em `plano-contas`
- `imposto`: obrigatorio em `fiscal/apuracoes/resumo`

## Compatibilidade de contrato

Para reduzir atrito com a documentacao operacional do cliente, a API expoe dois estilos de rota:

- rotas padronizadas em kebab-case, como `/api/oneflow/fiscal/documentos/listar`
- aliases compativeis com a nomenclatura legada dos materiais, como `/api/oneflow/fiscal/documentos/total/competencia`

Os aliases foram adicionados para:

- fiscal: `nfsenacional/incluir`, `nfseprefeitura/incluir`, `nfse/remove`, `nfsestatus/alterar`, `total/competencia`, `documentos/total/competencia`, `apuracao/competencia/imposto`, `aliquotas/simplesnacional/competencia`
- folha: `variaveisfolha`, `dadosbasicostrabalhador/competencia`, `eventosdostrabalhadores/competencia`, `statusfolha/competencia`, `datasconfigurada/competencia`, `fatorr/competencia`, `recibos/totais/competencia`
- contabilidade: `planocontas`, `lancamentos/conta`, `lancamentoscontabeis`, `lancamentoscontabeis/transacao`, `lancamentoscontabeis/padrao`, `lancamentoscontabeis/id`
- obrigacoes: `obrigacao/anexo`

## Observacoes importantes

- O repositorio nao versiona segredos.
- A autenticacao interna fica desabilitada ate que `INTERNAL_API_KEY` seja preenchida no `.env`.
- `App Key` e `App Secret` do Omie nao substituem o `ONEFLOW_COMPANY_TOKEN`.
- A API ficou preparada para renovar automaticamente o token da empresa quando `ONEFLOW_COMPANY_REFRESH_TOKEN` e `ONEFLOW_COMPANY_APP_HASH` forem informados.
- Os payloads `POST` foram mantidos flexiveis para receber o JSON final conforme a modelagem oficial do OneFlow.
- Os campos do Omie e do G-Click foram deixados preparados no `.env.example`, mas a integracao efetiva depende das credenciais reais do cliente.

## Referencias oficiais

- Autenticacao OneFlow via Omie: [ajuda.omie.com.br](https://ajuda.omie.com.br/pt-BR/articles/9113296-autenticacao-para-utilizacao-de-apis-do-oneflow)
- Especificacao SwaggerHub OneFlow 2.0.0: [api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0](https://api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0)
