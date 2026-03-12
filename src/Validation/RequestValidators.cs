using Microsoft.Extensions.Primitives;
using OneFlowApis.Models;

namespace OneFlowApis.Validation;

public static class RequestValidators
{
    public static string RequiredCompetencia(StringValues values, string fieldName)
    {
        var value = RequiredString(values, fieldName);
        if (value.Length != 6 || !value.All(char.IsDigit))
        {
            throw ValidationException(fieldName, "Competencia deve estar no formato AAAAMM.");
        }

        return value;
    }

    public static string RequiredString(StringValues values, string fieldName)
    {
        var value = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationException(fieldName, $"O campo {fieldName} e obrigatorio.");
        }

        return value;
    }

    public static string? OptionalString(StringValues values)
    {
        var value = values.ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static int RequiredPositiveInt(StringValues values, string fieldName)
    {
        var value = RequiredString(values, fieldName);
        if (!int.TryParse(value, out var parsedValue) || parsedValue <= 0)
        {
            throw ValidationException(fieldName, $"O campo {fieldName} deve ser um inteiro positivo.");
        }

        return parsedValue;
    }

    public static string RequiredAllowedValue(StringValues values, string fieldName, IReadOnlyCollection<string> allowedValues)
    {
        var value = RequiredString(values, fieldName);
        if (!allowedValues.Contains(value))
        {
            throw ValidationException(fieldName, $"O campo {fieldName} deve ser um dos valores: {string.Join(", ", allowedValues)}.");
        }

        return value;
    }

    public static void RequireAtLeastOne(string? firstValue, string? secondValue, string firstField, string secondField)
    {
        if (string.IsNullOrWhiteSpace(firstValue) && string.IsNullOrWhiteSpace(secondValue))
        {
            throw new AppException(400, "Falha de validacao da requisicao.", new
            {
                erros = new[]
                {
                    new
                    {
                        caminho = firstField,
                        mensagem = $"Informe ao menos {firstField} ou {secondField}."
                    }
                }
            });
        }
    }

    private static AppException ValidationException(string fieldName, string message)
    {
        return new AppException(400, "Falha de validacao da requisicao.", new
        {
            erros = new[]
            {
                new
                {
                    caminho = fieldName,
                    mensagem = message
                }
            }
        });
    }
}
