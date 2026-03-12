import { Router } from "express";

import { asyncHandler } from "../../shared/http/async-handler";
import { OneFlowController, oneFlowController } from "./oneflow.controller";

export const createOneFlowRouter = (controller: OneFlowController) => {
  const router = Router();

  router.get("/fiscal/documentos/totais", asyncHandler(controller.getDocumentosTotais.bind(controller)));
  router.post("/folha/variaveis", asyncHandler(controller.postVariaveisFolha.bind(controller)));
  router.get(
    "/folha/trabalhadores/dados-basicos",
    asyncHandler(controller.getTrabalhadorDadosBasicos.bind(controller)),
  );
  router.get("/folha/trabalhadores/eventos", asyncHandler(controller.getTrabalhadorEventos.bind(controller)));
  router.get("/folha/holerites/totais", asyncHandler(controller.getHoleritesTotais.bind(controller)));
  router.get("/folha/datas", asyncHandler(controller.getDatasFolha.bind(controller)));

  router.post("/contabilidade/lancamentos", asyncHandler(controller.postLancamentoContabil.bind(controller)));
  router.get("/contabilidade/balancete", asyncHandler(controller.getBalancete.bind(controller)));

  router.get("/guias/anexos", asyncHandler(controller.getObrigacoesAnexos.bind(controller)));

  return router;
};

export const oneFlowRouter = createOneFlowRouter(oneFlowController);
