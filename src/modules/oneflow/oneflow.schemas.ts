import { z } from "zod";

import { competenciaSchema, optionalStringSchema } from "../../shared/utils/validation";

export const documentosTotaisQuerySchema = z.object({
  competencia: competenciaSchema,
});

export const trabalhadorDadosBasicosQuerySchema = z
  .object({
    competencia: competenciaSchema,
    cpf: optionalStringSchema,
    matricula: optionalStringSchema,
  })
  .refine((data) => Boolean(data.cpf || data.matricula), {
    message: "Informe ao menos cpf ou matricula.",
    path: ["cpf"],
  });

export const trabalhadorEventosQuerySchema = trabalhadorDadosBasicosQuerySchema.extend({
  idEvento: optionalStringSchema,
});

export const holeritesTotaisQuerySchema = z.object({
  competencia: competenciaSchema,
  tipoFolha: z.coerce.number().int().positive(),
});

export const datasFolhaQuerySchema = z.object({
  competencia: competenciaSchema,
});

export const balanceteQuerySchema = z.object({
  competenciaInicial: competenciaSchema,
  competenciaFinal: competenciaSchema,
  zeramento: z.enum(["S", "N"]),
});

export const obrigacoesAnexosQuerySchema = z.object({
  competencia: competenciaSchema,
  codigo: z.string().trim().min(1),
});
