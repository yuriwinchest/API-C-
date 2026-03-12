import request from "supertest";
import { describe, expect, it } from "vitest";

import { createApp } from "../src/app";

describe("createApp", () => {
  it("deve responder healthcheck", async () => {
    const response = await request(createApp()).get("/health");

    expect(response.status).toBe(200);
    expect(response.body).toEqual({
      status: "ok",
      servico: "one-flow-apis",
    });
  });

  it("deve validar competencia no endpoint de documentos totais", async () => {
    const response = await request(createApp()).get("/api/oneflow/fiscal/documentos/totais?competencia=2025-01");

    expect(response.status).toBe(400);
    expect(response.body.mensagem).toBe("Falha de validação da requisição.");
  });
});
