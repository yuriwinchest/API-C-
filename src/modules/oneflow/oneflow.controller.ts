import { Request, Response } from "express";

import { OneFlowService, oneFlowService } from "./oneflow.service";

export class OneFlowController {
  constructor(private readonly service: OneFlowService) {}

  async getDocumentosTotais(request: Request, response: Response) {
    const result = await this.service.getDocumentosTotais(request.query);
    response.status(result.status).json(result.data);
  }

  async postVariaveisFolha(request: Request, response: Response) {
    const result = await this.service.postVariaveisFolha(request.body);
    response.status(result.status).json(result.data);
  }

  async getTrabalhadorDadosBasicos(request: Request, response: Response) {
    const result = await this.service.getTrabalhadorDadosBasicos(request.query);
    response.status(result.status).json(result.data);
  }

  async getTrabalhadorEventos(request: Request, response: Response) {
    const result = await this.service.getTrabalhadorEventos(request.query);
    response.status(result.status).json(result.data);
  }

  async getHoleritesTotais(request: Request, response: Response) {
    const result = await this.service.getHoleritesTotais(request.query);
    response.status(result.status).json(result.data);
  }

  async getDatasFolha(request: Request, response: Response) {
    const result = await this.service.getDatasFolha(request.query);
    response.status(result.status).json(result.data);
  }

  async postLancamentoContabil(request: Request, response: Response) {
    const result = await this.service.postLancamentoContabil(request.body);
    response.status(result.status).json(result.data);
  }

  async getBalancete(request: Request, response: Response) {
    const result = await this.service.getBalancete(request.query);
    response.status(result.status).json(result.data);
  }

  async getObrigacoesAnexos(request: Request, response: Response) {
    const result = await this.service.getObrigacoesAnexos(request.query);
    response.status(result.status).json(result.data);
  }
}

export const oneFlowController = new OneFlowController(oneFlowService);
