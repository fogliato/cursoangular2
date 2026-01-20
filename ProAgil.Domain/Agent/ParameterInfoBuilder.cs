using System;
using System.Reflection;
using System.Text;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Utility for building parameter information
    /// </summary>
    public static class ParameterInfoBuilder
    {
        public static string BuildParameterInfo(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                var paramTypeName = param.ParameterType.Name;
                sb.AppendLine($"\n## Parameter: {param.Name} (Type: {paramTypeName})");

                // If it's a complex object, list its properties with more details
                if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
                {
                    var properties = param.ParameterType.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    if (properties.Length > 0)
                    {
                        sb.AppendLine($"Properties of {paramTypeName}:");
                        foreach (var prop in properties)
                        {
                            var propTypeName = prop.PropertyType.Name;

                            // Check if nullable
                            var isNullable =
                                Nullable.GetUnderlyingType(prop.PropertyType) != null
                                || !prop.PropertyType.IsValueType;
                            var nullableIndicator = isNullable
                                ? " (optional, can be null)"
                                : " (required)";

                            // Special types
                            if (
                                prop.PropertyType == typeof(DateTime)
                                || prop.PropertyType == typeof(DateTime?)
                            )
                            {
                                sb.AppendLine(
                                    $"  - {prop.Name}: {propTypeName}{nullableIndicator}"
                                );
                                sb.AppendLine(
                                    $"    Format: 'yyyy-MM-ddTHH:mm:ss' (e.g.: '2025-11-18T10:30:00')"
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
                                    $"  - {prop.Name}: List of {innerType}{nullableIndicator}"
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
                    sb.AppendLine($"Simple type: {paramTypeName}");
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
