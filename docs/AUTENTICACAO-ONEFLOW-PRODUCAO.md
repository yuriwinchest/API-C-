# AUTENTICACAO ONEFLOW EM PRODUCAO

## OBJETIVO

Este documento explica, de forma operacional, como gerar o conjunto inicial de credenciais do OneFlow e por que a API foi ajustada para renovar o token automaticamente em producao.

## O QUE FOI ALTERADO

As ultimas alteracoes na API deixaram a autenticacao preparada para uso real em producao:

- a API identifica quando o token da empresa expira
- a API renova automaticamente o token com `refresh_token` e `app_hash`
- a API repete a requisicao original depois da renovacao
- a API persiste o novo `token` e o novo `refresh_token` no `.env`, quando houver permissao de escrita
- a API expoe endpoints internos de diagnostico e refresh manual para suporte operacional

## POR QUE FOI ALTERADO

O OneFlow/Omie nao trabalha com token definitivo.

Isso significa que:

- um JWT fixo nao deve ser tratado como credencial permanente
- se o ambiente subir apenas com um token antigo e sem refresh configurado, a integracao vai parar quando esse token expirar
- o modo correto de operacao em producao e trabalhar com o conjunto `token + refresh_token + app_hash`

Por isso, a geracao manual serve apenas para montar o conjunto inicial de credenciais. Depois disso, a API passa a cuidar da renovacao automaticamente.

## COMO FUNCIONA AGORA

Em producao, o fluxo correto fica assim:

1. gerar o conjunto inicial da empresa
2. preencher `ONEFLOW_COMPANY_TOKEN`, `ONEFLOW_COMPANY_REFRESH_TOKEN` e `ONEFLOW_COMPANY_APP_HASH`
3. subir a API com esse `.env`
4. deixar a API renovar o token automaticamente quando houver expiracao

## CAMPOS NECESSARIOS NO .ENV

No final do processo, estes sao os campos que precisam estar configurados:

```env
ONEFLOW_COMPANY_TOKEN=SEU_TOKEN_DA_EMPRESA
ONEFLOW_COMPANY_REFRESH_TOKEN=SEU_REFRESH_TOKEN_DA_EMPRESA
ONEFLOW_COMPANY_APP_HASH=SEU_APP_HASH_DA_EMPRESA
```

## PASSO A PASSO OPERACIONAL PARA GERAR O CONJUNTO INICIAL

### PASSO 1 - FAZER LOGIN

Entrar com o usuario que possui acesso ao OneFlow e as empresas:

- [Portal OneFlow](https://portal.oneflow.com.br)
- [Portal Omie](https://app.omie.com.br)

### PASSO 2 - GERAR O TOKEN DO USUARIO

Abrir o link abaixo com o usuario logado:

- [https://app.omie.com.br/api/portal/users/me/token/](https://app.omie.com.br/api/portal/users/me/token/)

Esse retorno vai trazer o token do usuario. Guardar:

- `TOKEN_DO_USUARIO`

### PASSO 3 - LISTAR OS APLICATIVOS DISPONIVEIS

Consultar:

- [https://app.omie.com.br/api/portal/apps/](https://app.omie.com.br/api/portal/apps/)

Exemplo:

```bash
curl -X GET "https://app.omie.com.br/api/portal/apps/" \
  -H "Authorization: Bearer TOKEN_DO_USUARIO" \
  -H "Accept: application/json"
```

Na resposta, localizar o aplicativo com:

- `app_type = "ONEFLOW"`

Guardar:

- `APP_HASH_DO_ESCRITORIO`

### PASSO 4 - GERAR O TOKEN DO APLICATIVO ONEFLOW DO ESCRITORIO

Consultar:

- `https://app.omie.com.br/api/portal/apps/APP_HASH_DO_ESCRITORIO/token/`

Exemplo:

```bash
curl -X GET "https://app.omie.com.br/api/portal/apps/APP_HASH_DO_ESCRITORIO/token/" \
  -H "Authorization: Bearer TOKEN_DO_USUARIO" \
  -H "Accept: application/json"
```

Guardar:

- `TOKEN_DO_ESCRITORIO`

### PASSO 5 - LISTAR AS EMPRESAS DISPONIVEIS

Consultar:

- `https://rest.oneflow.com.br/api/oneflow/escritorio/empresas/listar?pagina=1`

Exemplo:

```bash
curl -X GET "https://rest.oneflow.com.br/api/oneflow/escritorio/empresas/listar?pagina=1" \
  -H "Authorization: Bearer TOKEN_DO_ESCRITORIO" \
  -H "Accept: application/json"
```

Na resposta, localizar a empresa correta e guardar:

- `APP_HASH_DA_EMPRESA`

### PASSO 6 - GERAR O TOKEN FINAL DA EMPRESA

Consultar:

- `https://app.omie.com.br/api/portal/apps/APP_HASH_DA_EMPRESA/token/`

Exemplo:

```bash
curl -X GET "https://app.omie.com.br/api/portal/apps/APP_HASH_DA_EMPRESA/token/" \
  -H "Authorization: Bearer TOKEN_DO_USUARIO" \
  -H "Accept: application/json"
```

Essa resposta vai trazer os dados finais usados pela API:

- `token`
- `refresh_token`

No `.env`, esses valores devem ser mapeados para:

- `ONEFLOW_COMPANY_TOKEN`
- `ONEFLOW_COMPANY_REFRESH_TOKEN`
- `ONEFLOW_COMPANY_APP_HASH`

## O QUE ACONTECE DEPOIS DISSO

Depois que o conjunto inicial estiver no `.env`:

- a API usa o token atual para chamar o OneFlow
- se o OneFlow responder expiracao ou `401`, a API tenta renovar automaticamente
- a API refaz a mesma requisicao com o token renovado
- se o `.env` puder ser gravado, a API salva o novo token e o novo refresh token no arquivo

## ENDPOINTS INTERNOS DE SUPORTE

Os endpoints internos adicionados para operacao e suporte sao:

- `GET /api/oneflow/configuracao/status`
- `GET /api/oneflow/autenticacao/status`
- `POST /api/oneflow/autenticacao/refresh`

## ERROS MAIS COMUNS

### O APP ONEFLOW NAO APARECE

Normalmente significa falta de permissao do usuario logado no ambiente.

### A EMPRESA NAO APARECE NA LISTAGEM

Normalmente significa falta de permissao do escritorio para essa empresa ou falta de acesso do usuario.

### O TOKEN EXPIRA DEPOIS DA PUBLICACAO

Esse comportamento e esperado no OneFlow/Omie. O ambiente correto deve estar com `refresh_token` e `app_hash` configurados para a API renovar automaticamente.

## CHECKLIST FINAL

Antes de publicar, confirmar:

1. `ONEFLOW_COMPANY_TOKEN` configurado
2. `ONEFLOW_COMPANY_REFRESH_TOKEN` configurado
3. `ONEFLOW_COMPANY_APP_HASH` configurado
4. `INTERNAL_API_KEY` configurada
5. `GET /api/oneflow/autenticacao/status` indicando renovacao automatica habilitada
6. `pwsh -File .\scripts\FinalSmokeTest.ps1` executado com sucesso
