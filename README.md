# One-Flow-APIs

API em Node.js + TypeScript para integrar os endpoints do OneFlow necessários para folha, contabilidade e guias/obrigações.

## Objetivo

Este projeto expõe uma API interna organizada e validada, que faz proxy para os endpoints oficiais do OneFlow. O token JWT da empresa fica isolado em variável de ambiente e nao deve ser versionado.

## Stack

- Node.js 22+
- TypeScript
- Express
- Zod
- Vitest

## Configuração

1. Copie o arquivo `.env.example` para `.env`.
2. Preencha `ONEFLOW_COMPANY_TOKEN` com o JWT final da empresa no OneFlow.
3. Instale as dependências:

```bash
npm install
```

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
| --- | --- | --- |
| `PORT` | Não | Porta local da API. Padrão: `3000`. |
| `ONEFLOW_BASE_URL` | Não | Base da API oficial. Padrão: `https://rest.oneflow.com.br/api`. |
| `ONEFLOW_COMPANY_TOKEN` | Sim | Token JWT da empresa usado no header `Authorization: Bearer <token>`. |
| `OMIE_PORTAL_APPS_BASE_URL` | Nao | Base do portal Omie usada na renovacao do token. |
| `ONEFLOW_COMPANY_REFRESH_TOKEN` | Recomendado | Refresh token da empresa para renovacao automatica. |
| `ONEFLOW_COMPANY_APP_HASH` | Recomendado | `app_hash` da empresa no OneFlow, necessario para renovar o token. |

## Execução

Desenvolvimento:

```bash
npm run dev
```

Build:

```bash
npm run build
```

Produção local:

```bash
npm start
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

| Método | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/folha/variaveis` | `/oneflow/empresa/folha/variaveis/incluir` |
| `GET` | `/api/oneflow/folha/trabalhadores/dados-basicos` | `/oneflow/empresa/folha/trabalhador/dadosbasicos` |
| `GET` | `/api/oneflow/folha/trabalhadores/eventos` | `/oneflow/empresa/folha/trabalhador/eventos` |
| `GET` | `/api/oneflow/folha/holerites/totais` | `/oneflow/empresa/folha/recibos/totais` |
| `GET` | `/api/oneflow/folha/datas` | `/oneflow/empresa/folha/datas` |

### Fiscal

| Método | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/fiscal/documentos/totais` | `/oneflow/empresa/fiscal/documentos/totais` |

### Contabilidade

| Método | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `POST` | `/api/oneflow/contabilidade/lancamentos` | `/oneflow/empresa/contabil/lancamentos/gerarlancamento` |
| `GET` | `/api/oneflow/contabilidade/balancete` | `/oneflow/empresa/contabil/balancete` |

### Guias e obrigações

| Método | Endpoint interno | Endpoint OneFlow |
| --- | --- | --- |
| `GET` | `/api/oneflow/guias/anexos` | `/oneflow/empresa/obrigacoes/anexos` |

## Parâmetros validados

### `GET /api/oneflow/fiscal/documentos/totais`

- `competencia`: formato `AAAAMM`

### `GET /api/oneflow/folha/trabalhadores/dados-basicos`

- `competencia`: formato `AAAAMM`
- `cpf` ou `matricula`: pelo menos um dos dois deve ser informado

### `GET /api/oneflow/folha/trabalhadores/eventos`

- `competencia`: formato `AAAAMM`
- `cpf` ou `matricula`: pelo menos um dos dois deve ser informado
- `idEvento`: opcional

### `GET /api/oneflow/folha/holerites/totais`

- `competencia`: formato `AAAAMM`
- `tipoFolha`: inteiro positivo

### `GET /api/oneflow/folha/datas`

- `competencia`: formato `AAAAMM`

### `GET /api/oneflow/contabilidade/balancete`

- `competenciaInicial`: formato `AAAAMM`
- `competenciaFinal`: formato `AAAAMM`
- `zeramento`: `S` ou `N`

### `GET /api/oneflow/guias/anexos`

- `competencia`: formato `AAAAMM`
- `codigo`: codigo da obrigacao no OneFlow

## Exemplos de uso

Buscar totais de documentos fiscais:

```bash
curl "http://localhost:3000/api/oneflow/fiscal/documentos/totais?competencia=202501"
```

Buscar dados basicos de trabalhador:

```bash
curl "http://localhost:3000/api/oneflow/folha/trabalhadores/dados-basicos?competencia=202501&cpf=000.000.000-00"
```

Buscar holerites:

```bash
curl "http://localhost:3000/api/oneflow/folha/holerites/totais?competencia=202501&tipoFolha=1"
```

Lancar contabilmente:

```bash
curl -X POST "http://localhost:3000/api/oneflow/contabilidade/lancamentos" ^
  -H "Content-Type: application/json" ^
  -d "{\"codigoIntegracao\":\"ABC123\"}"
```

## Testes

```bash
npm test
```

## Referências oficiais

- Autenticacao OneFlow via Omie: [ajuda.omie.com.br](https://ajuda.omie.com.br/pt-BR/articles/9113296-autenticacao-para-utilizacao-de-apis-do-oneflow)
- Especificação SwaggerHub OneFlow 2.0.0: [api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0](https://api.swaggerhub.com/apis/oneflowoficial/integracoes/2.0.0)

## Observações importantes

- O token nao foi preenchido neste repositório.
- `App Key` e `App Secret` do Omie nao substituem o `ONEFLOW_COMPANY_TOKEN`. Para consumir as APIs do OneFlow, ainda é necessario obter o JWT da empresa conforme o fluxo oficial de autenticacao do OneFlow.
- A API ficou preparada para renovar automaticamente o token da empresa quando `ONEFLOW_COMPANY_REFRESH_TOKEN` e `ONEFLOW_COMPANY_APP_HASH` forem informados.
- Os payloads `POST` foram mantidos flexíveis para receber o JSON final conforme a modelagem oficial do OneFlow.
- Antes de subir para GitHub, revise o `.env` para garantir que nenhuma credencial foi versionada.
