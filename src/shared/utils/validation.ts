import { z } from "zod";

const competenciaRegex = /^\d{6}$/;

export const competenciaSchema = z
  .string()
  .regex(competenciaRegex, "Competência deve estar no formato AAAAMM.");

export const optionalStringSchema = z.string().trim().min(1).optional();
