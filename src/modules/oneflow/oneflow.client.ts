import { env } from "../../config/env";
import { AppError } from "../../shared/errors/app-error";
import { oneFlowAuthManager } from "./oneflow.auth";
import { OneFlowRequestOptions, OneFlowResponse } from "./oneflow.types";

const buildUrl = (path: string, query?: Record<string, string | number | undefined>) => {
  const url = new URL(path, env.ONEFLOW_BASE_URL.endsWith("/") ? env.ONEFLOW_BASE_URL : `${env.ONEFLOW_BASE_URL}/`);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) {
        url.searchParams.set(key, String(value));
      }
    }
  }

  return url;
};

const parseResponse = async (response: Response) => {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    return response.json();
  }

  return response.text();
};

export class OneFlowClient {
  private async executeRequest<T = unknown>(
    options: OneFlowRequestOptions,
    token: string,
  ): Promise<OneFlowResponse<T>> {
    const url = buildUrl(options.path, options.query);
    const response = await fetch(url, {
      method: options.method,
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: options.body ? JSON.stringify(options.body) : undefined,
    });

    const data = await parseResponse(response);

    if (!response.ok) {
      throw new AppError(response.status, "Falha ao consumir a API do OneFlow.", data);
    }

    return {
      status: response.status,
      data: data as T,
    };
  }

  async request<T = unknown>(options: OneFlowRequestOptions): Promise<OneFlowResponse<T>> {
    const currentToken = oneFlowAuthManager.getToken() ?? env.ONEFLOW_COMPANY_TOKEN;

    if (!currentToken) {
      throw new AppError(
        500,
        "Variavel ONEFLOW_COMPANY_TOKEN nao configurada. Preencha o token JWT da empresa para habilitar a integracao.",
      );
    }

    try {
      return await this.executeRequest(options, currentToken);
    } catch (error) {
      if (error instanceof AppError && error.statusCode === 401 && oneFlowAuthManager.canRefresh()) {
        await oneFlowAuthManager.refreshCompanyToken();
        return this.executeRequest(options, oneFlowAuthManager.getToken() as string);
      }

      if (error instanceof AppError) {
        throw error;
      }

      throw new AppError(502, "Erro de comunicacao com a API do OneFlow.", {
        causa: error instanceof Error ? error.message : "erro_desconhecido",
      });
    }
  }
}

export const oneFlowClient = new OneFlowClient();
