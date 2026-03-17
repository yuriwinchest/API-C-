# Integracao do Cliente

## Objetivo

Este documento orienta a integracao da API com o sistema externo do cliente sem versionar segredos no GitHub.

## O QUE FOI ALTERADO

As ultimas alteracoes deixaram a autenticacao preparada para operacao real em producao:

- renovacao automatica do token da empresa
- persistencia do token renovado no `.env`, quando houver permissao
- endpoint interno para diagnostico do token
- endpoint interno para refresh manual

## POR QUE ISSO FOI ALTERADO

O OneFlow/Omie nao trabalha com token definitivo.

Na pratica, isso significa que:

- um JWT isolado nao deve ser tratado como credencial permanente
- se a aplicacao subir sem `refresh_token` e `app_hash`, ela pode parar quando o token expirar
- o fluxo correto para producao e gerar o conjunto inicial e deixar a API renovar automaticamente

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

## Renovacao automatica do token

Quando `ONEFLOW_COMPANY_TOKEN`, `ONEFLOW_COMPANY_REFRESH_TOKEN` e `ONEFLOW_COMPANY_APP_HASH` estao configurados:

- a API tenta renovar automaticamente o token quando o OneFlow responder `401`
- a mesma requisicao e repetida com o token renovado
- se a aplicacao estiver usando um arquivo `.env` com permissao de escrita, o novo `token` e o novo `refresh_token` sao salvos automaticamente nele
- o endpoint `GET /api/oneflow/autenticacao/status` mostra o estado do token e da persistencia
- o endpoint `POST /api/oneflow/autenticacao/refresh` permite forcar uma renovacao manual em caso de suporte ou validacao operacional

## Endpoints iniciais recomendados para homologacao

- `GET /api/oneflow/configuracao/status`
- `GET /api/oneflow/autenticacao/status`
- `GET /api/oneflow/guias/obrigacoes/geral`
- `GET /api/oneflow/escritorio/empresas/listar?pagina=1`

## Como funciona a geracao inicial do token

Para a primeira carga do ambiente, ainda e necessario usar o fluxo oficial do OneFlow/Omie para obter o conjunto inicial de:

- `ONEFLOW_COMPANY_TOKEN`
- `ONEFLOW_COMPANY_REFRESH_TOKEN`
- `ONEFLOW_COMPANY_APP_HASH`

Depois que esse conjunto inicial estiver no `.env`, a operacao normal deixa de depender de geracao manual frequente. A API passa a renovar o token automaticamente.

O passo a passo operacional completo, com links e exemplos de chamada, esta em [AUTENTICACAO-ONEFLOW-PRODUCAO.md](./AUTENTICACAO-ONEFLOW-PRODUCAO.md).

## Contrato homologado para integracao

### Modulo: Folha de Pagamento e Trabalhador

- Detalhamento de notas: `GET /api/oneflow/fiscal/documentos/total/competencia`
- Variaveis da folha: `POST /api/oneflow/folha/variaveisfolha`
- Dados basicos do trabalhador: `GET /api/oneflow/folha/dadosbasicostrabalhador/competencia`
- Eventos do trabalhador e ferias: `GET /api/oneflow/folha/eventosdostrabalhadores/competencia`
- Holerites e totais de recibos: `GET /api/oneflow/folha/holerites/totais/competencia`
- Datas da folha: `GET /api/oneflow/folha/datasconfigurada/competencia`

### Modulo: Contabilidade e BI

- Inclusao de lancamentos contabeis: `POST /api/oneflow/contabilidade/lancamentoscontabeis`
- Geracao de balancete e relatorios contabeis: `GET /api/oneflow/contabilidade/balancete`

### Modulo: Guias e Obrigacoes

- Listagem geral de guias e obrigacoes: `GET /api/oneflow/guias/obrigacoes/geral`
- Consulta de anexos por competencia e codigo: `GET /api/oneflow/obrigacao/anexo`

## Consulta de empresa cadastrada

- dados basicos da empresa atual: `GET /api/oneflow/empresa/dados-basicos`
- listagem de empresas do escritorio: `GET /api/oneflow/escritorio/empresas/listar?pagina=1`
- detalhes por CNPJ: `GET /api/oneflow/escritorio/empresas/detalhes?cnpj=62907967000109`

## Evidencia minima esperada antes da homologacao

- `GET /health` retorna `200`
- `GET /swagger/v1/swagger.json` retorna `200`
- endpoint interno sem header retorna `401`
- `GET /api/oneflow/configuracao/status` retorna que o ambiente esta pronto para teste
- `GET /api/oneflow/autenticacao/status` retorna que a renovacao automatica esta habilitada
- `GET /api/oneflow/guias/obrigacoes/geral` retorna dados reais do OneFlow

## Observacao final

Se o frontend do cliente estiver em outro projeto, a integracao deve ser feita pelo backend desse projeto. Esta API nao foi preparada para expor a chave interna no navegador.
