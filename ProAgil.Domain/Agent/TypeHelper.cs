using System;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Utilities for working with types
    /// </summary>
    public static class TypeHelper
    {
        public static string GetExampleValue(Type type)
        {
            if (type == typeof(string))
                return "\"example\"";
            if (type == typeof(int) || type == typeof(int?))
                return "123";
            if (type == typeof(bool) || type == typeof(bool?))
                return "true";
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return $"\"{DateTime.Now:yyyy-MM-dd}T00:00:00\"";
            if (
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)
            )
                return "[]";

            return "null";
        }
    }
}
