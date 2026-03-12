import http from "node:http";
import https from "node:https";

import { z } from "zod";

import { env } from "../../config/env";
import { AppError } from "../../shared/errors/app-error";

const refreshResponseSchema = z.object({
  token: z.string().min(1),
  refresh_token: z.string().min(1),
});

type OneFlowAuthState = {
  token?: string;
  refreshToken?: string;
};

const authState: OneFlowAuthState = {
  token: env.ONEFLOW_COMPANY_TOKEN,
  refreshToken: env.ONEFLOW_COMPANY_REFRESH_TOKEN,
};

const readResponseBody = (response: http.IncomingMessage) =>
  new Promise<string>((resolve, reject) => {
    let body = "";

    response.setEncoding("utf8");
    response.on("data", (chunk) => {
      body += chunk;
    });
    response.on("end", () => resolve(body));
    response.on("error", reject);
  });

const sendRefreshRequest = async (url: URL, payload: string) => {
  const transport = url.protocol === "https:" ? https : http;

  return new Promise<{ statusCode: number; body: string }>((resolve, reject) => {
    const request = transport.request(
      url,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          "Content-Length": Buffer.byteLength(payload),
        },
      },
      async (response) => {
        try {
          const body = await readResponseBody(response);
          resolve({
            statusCode: response.statusCode ?? 500,
            body,
          });
        } catch (error) {
          reject(error);
        }
      },
    );

    request.on("error", reject);
    request.write(payload);
    request.end();
  });
};

class OneFlowAuthManager {
  getToken() {
    return authState.token;
  }

  canRefresh() {
    return Boolean(authState.token && authState.refreshToken && env.ONEFLOW_COMPANY_APP_HASH);
  }

  async refreshCompanyToken() {
    if (!this.canRefresh()) {
      throw new AppError(
        500,
        "Renovacao automatica indisponivel. Configure ONEFLOW_COMPANY_TOKEN, ONEFLOW_COMPANY_REFRESH_TOKEN e ONEFLOW_COMPANY_APP_HASH.",
      );
    }

    const url = new URL(
      `${env.OMIE_PORTAL_APPS_BASE_URL.replace(/\/$/, "")}/${env.ONEFLOW_COMPANY_APP_HASH}/refresh-token`,
    );
    const payload = JSON.stringify({
      token: authState.token,
      refresh_token: authState.refreshToken,
    });

    const response = await sendRefreshRequest(url, payload);
    let parsedBody: unknown = response.body;

    try {
      parsedBody = JSON.parse(response.body);
    } catch {
      parsedBody = response.body;
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw new AppError(response.statusCode, "Falha ao renovar o token da empresa no portal Omie.", parsedBody);
    }

    const tokens = refreshResponseSchema.parse(parsedBody);
    authState.token = tokens.token;
    authState.refreshToken = tokens.refresh_token;

    return tokens;
  }
}

export const oneFlowAuthManager = new OneFlowAuthManager();
