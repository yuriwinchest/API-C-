import express from "express";

import { oneFlowRouter } from "./modules/oneflow/oneflow.routes";
import { errorHandler } from "./shared/http/error-handler";

export const createApp = () => {
  const app = express();

  app.use(express.json({ limit: "2mb" }));

  app.get("/health", (_request, response) => {
    response.status(200).json({
      status: "ok",
      servico: "one-flow-apis",
    });
  });

  app.use("/api/oneflow", oneFlowRouter);
  app.use(errorHandler);

  return app;
};
