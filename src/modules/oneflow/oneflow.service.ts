import { OneFlowClient, oneFlowClient } from "./oneflow.client";
import {
  balanceteQuerySchema,
  datasFolhaQuerySchema,
  documentosTotaisQuerySchema,
  holeritesTotaisQuerySchema,
  obrigacoesAnexosQuerySchema,
  trabalhadorDadosBasicosQuerySchema,
  trabalhadorEventosQuerySchema,
} from "./oneflow.schemas";

export class OneFlowService {
  constructor(private readonly client: OneFlowClient) {}

  getDocumentosTotais(query: unknown) {
    const parsed = documentosTotaisQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/fiscal/documentos/totais",
      query: parsed,
    });
  }

  postVariaveisFolha(body: unknown) {
    return this.client.request({
      method: "POST",
      path: "oneflow/empresa/folha/variaveis/incluir",
      body,
    });
  }

  getTrabalhadorDadosBasicos(query: unknown) {
    const parsed = trabalhadorDadosBasicosQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/folha/trabalhador/dadosbasicos",
      query: parsed,
    });
  }

  getTrabalhadorEventos(query: unknown) {
    const parsed = trabalhadorEventosQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/folha/trabalhador/eventos",
      query: parsed,
    });
  }

  getHoleritesTotais(query: unknown) {
    const parsed = holeritesTotaisQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/folha/recibos/totais",
      query: parsed,
    });
  }

  getDatasFolha(query: unknown) {
    const parsed = datasFolhaQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/folha/datas",
      query: parsed,
    });
  }

  postLancamentoContabil(body: unknown) {
    return this.client.request({
      method: "POST",
      path: "oneflow/empresa/contabil/lancamentos/gerarlancamento",
      body,
    });
  }

  getBalancete(query: unknown) {
    const parsed = balanceteQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/contabil/balancete",
      query: parsed,
    });
  }

  getObrigacoesAnexos(query: unknown) {
    const parsed = obrigacoesAnexosQuerySchema.parse(query);
    return this.client.request({
      method: "GET",
      path: "oneflow/empresa/obrigacoes/anexos",
      query: parsed,
    });
  }
}

export const oneFlowService = new OneFlowService(oneFlowClient);
