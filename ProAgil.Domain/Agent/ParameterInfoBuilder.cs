using System;
using System.Reflection;
using System.Text;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Utilitário para construir informações sobre parâmetros
    /// </summary>
    public static class ParameterInfoBuilder
    {
        public static string BuildParameterInfo(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                var paramTypeName = param.ParameterType.Name;
                sb.AppendLine($"\n## Parâmetro: {param.Name} (Tipo: {paramTypeName})");

                // Se for um objeto complexo, listar suas propriedades com mais detalhes
                if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
                {
                    var properties = param.ParameterType.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    if (properties.Length > 0)
                    {
                        sb.AppendLine($"Propriedades de {paramTypeName}:");
                        foreach (var prop in properties)
                        {
                            var propTypeName = prop.PropertyType.Name;

                            // Verificar se é nullable
                            var isNullable =
                                Nullable.GetUnderlyingType(prop.PropertyType) != null
                                || !prop.PropertyType.IsValueType;
                            var nullableIndicator = isNullable
                                ? " (opcional, pode ser null)"
                                : " (obrigatório)";

                            // Tipos especiais
                            if (
                                prop.PropertyType == typeof(DateTime)
                                || prop.PropertyType == typeof(DateTime?)
                            )
                            {
                                sb.AppendLine(
                                    $"  - {prop.Name}: {propTypeName}{nullableIndicator}"
                                );
                                sb.AppendLine(
                                    $"    Formato: 'yyyy-MM-ddTHH:mm:ss' (ex: '2025-11-18T10:30:00')"
                                );
                            }
                            else if (
                                prop.PropertyType.IsGenericType
                                && prop.PropertyType.GetGenericTypeDefinition()
                                    == typeof(System.Collections.Generic.List<>)
                            )
                            {
                                var innerType = prop.PropertyType.GetGenericArguments()[0].Name;
                                sb.AppendLine(
                                    $"  - {prop.Name}: Lista de {innerType}{nullableIndicator}"
                                );
                            }
                            else
                            {
                                sb.AppendLine(
                                    $"  - {prop.Name}: {propTypeName}{nullableIndicator}"
                                );
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"Tipo simples: {paramTypeName}");
                }
            }
            return sb.ToString();
        }

        public static string GetTypePropertiesDescription(Type type)
        {
            var sb = new StringBuilder();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties.Take(20))
            {
                sb.AppendLine($"- {prop.Name}: {prop.PropertyType.Name}");
            }

            return sb.ToString();
        }
    }
}
