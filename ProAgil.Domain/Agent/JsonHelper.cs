using System;
using System.Text.Json;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Utilitários para manipulação de JSON
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public static string CleanJsonResponse(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return json ?? string.Empty;

            json = json.Trim();
            if (json.StartsWith("```json"))
            {
                json = json.Substring(7);
            }
            if (json.StartsWith("```"))
            {
                json = json.Substring(3);
            }
            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }
            return json.Trim();
        }

        public static T Deserialize<T>(string json)
        {
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (result == null)
            {
                throw new InvalidOperationException("Deserialization returned null.");
            }
            return result;
        }

        public static object? DeserializeParameter(JsonElement element, System.Type targetType)
        {
            try
            {
                return JsonSerializer.Deserialize(element.GetRawText(), targetType, JsonOptions);
            }
            catch
            {
                return targetType.IsValueType ? System.Activator.CreateInstance(targetType) : null;
            }
        }

        public static string Serialize(object? obj)
        {
            return JsonSerializer.Serialize(obj, JsonOptions);
        }

        public static JsonElement DeserializeToElement(string json)
        {
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }

        public static object? ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElement(prop.Value);
                    }
                    return dict;

                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long l))
                        return l;
                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                    return null;

                default:
                    return element.ToString();
            }
        }
    }
}
