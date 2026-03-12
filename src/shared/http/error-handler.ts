import { NextFunction, Request, Response } from "express";
import { ZodError } from "zod";

import { AppError } from "../errors/app-error";

export const errorHandler = (
  error: unknown,
  _request: Request,
  response: Response,
  _next: NextFunction,
) => {
  if (error instanceof ZodError) {
    return response.status(400).json({
      mensagem: "Falha de validação da requisição.",
      erros: error.issues.map((issue) => ({
        caminho: issue.path.join("."),
        mensagem: issue.message,
      })),
    });
  }

  if (error instanceof AppError) {
    return response.status(error.statusCode).json({
      mensagem: error.message,
      detalhes: error.details ?? null,
    });
  }

  return response.status(500).json({
    mensagem: "Erro interno não tratado.",
  });
};
