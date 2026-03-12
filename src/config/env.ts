import dotenv from "dotenv";
import { z } from "zod";

dotenv.config();

const envSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: z.coerce.number().int().positive().default(3000),
  ONEFLOW_BASE_URL: z.string().url().default("https://rest.oneflow.com.br/api"),
  OMIE_PORTAL_APPS_BASE_URL: z.string().url().default("https://app.omie.com.br/api/portal/apps"),
  ONEFLOW_COMPANY_TOKEN: z.string().trim().optional(),
  ONEFLOW_COMPANY_REFRESH_TOKEN: z.string().trim().optional(),
  ONEFLOW_COMPANY_APP_HASH: z.string().trim().optional(),
});

const parsedEnv = envSchema.safeParse(process.env);

if (!parsedEnv.success) {
  throw new Error(`Falha ao carregar variáveis de ambiente: ${parsedEnv.error.message}`);
}

export const env = parsedEnv.data;
