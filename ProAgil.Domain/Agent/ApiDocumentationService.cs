using System;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Service responsible for generating API documentation
    /// </summary>
    public interface IApiDocumentationService
    {
        string GenerateApiDocumentation();
    }

    public class ApiDocumentationService : IApiDocumentationService
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;

        public ApiDocumentationService(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            _apiExplorer = apiExplorer;
        }

        public string GenerateApiDocumentation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Available API Endpoints\n");

            // Use API Explorer to get all endpoints with Swagger documentation
            var apiDescriptions = _apiExplorer
                .ApiDescriptionGroups.Items.SelectMany(g => g.Items)
                .Where(api =>
                    !api.RelativePath.Contains("agent", StringComparison.OrdinalIgnoreCase)
                )
                .OrderBy(api => api.RelativePath);

            var groupedByController = apiDescriptions.GroupBy(api =>
            {
                var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                return controllerActionDescriptor?.ControllerName ?? "Unknown";
            });

            foreach (var controllerGroup in groupedByController)
            {
                sb.AppendLine($"\n## Controller: {controllerGroup.Key}");
                sb.AppendLine(new string('-', 50));

                foreach (var api in controllerGroup)
                {
                    var actionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                    if (actionDescriptor == null)
                        continue;

                    var httpMethod = api.HttpMethod ?? "GET";
                    var actionName = actionDescriptor.ActionName;
                    var route = api.RelativePath;

                    sb.AppendLine($"\n### [{httpMethod}] {actionName}");
                    sb.AppendLine($"Route: {route}");

                    // Get XML documentation from method
                    var methodInfo = actionDescriptor.MethodInfo;
                    var xmlDoc = GetXmlDocumentation(methodInfo);
                    if (!string.IsNullOrEmpty(xmlDoc))
                    {
                        sb.AppendLine($"Description: {xmlDoc}");
                    }

                    // List parameters with their descriptions
                    if (api.ParameterDescriptions.Any())
                    {
                        sb.AppendLine("Parameters:");
                        foreach (var param in api.ParameterDescriptions)
                        {
                            var paramType = param.Type?.Name ?? "object";
                            var paramName = param.Name;
                            var source = param.Source.DisplayName;

                            sb.AppendLine($"  - {paramName} ({paramType}) [{source}]");

                            // If it's a complex object, list its properties
                            if (
                                param.Type != null
                                && param.Type.IsClass
                                && param.Type != typeof(string)
                            )
                            {
                                var properties = param.Type.GetProperties(
                                    System.Reflection.BindingFlags.Public
                                        | System.Reflection.BindingFlags.Instance
                                );
                                if (properties.Length > 0 && properties.Length <= 20)
                                {
                                    sb.AppendLine($"    Properties of {paramType}:");
                                    foreach (var prop in properties)
                                    {
                                        sb.AppendLine(
                                            $"      - {prop.Name}: {prop.PropertyType.Name}"
                                        );
                                    }
                                }
                            }
                        }
                    }

                    sb.AppendLine();
                }
            }

            var documentation = sb.ToString();
            Console.WriteLine(
                $"[ApiDocumentationService] Complete documentation generated: {documentation.Length} characters, {groupedByController.Count()} controllers"
            );
            return documentation;
        }

        private string GetXmlDocumentation(System.Reflection.MethodInfo method)
        {
            try
            {
                // Try to get from [Description]
                var descAttr =
                    method
                        .GetCustomAttributes(false)
                        .FirstOrDefault(a => a.GetType().Name == "DescriptionAttribute")
                    as DescriptionAttribute;

                if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    return descAttr.Description;

                // Try to get from XML tags via reflection in the assembly
                var xmlFile = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{method.DeclaringType?.Assembly.GetName().Name}.xml"
                );

                if (File.Exists(xmlFile))
                {
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.Load(xmlFile);

                    var memberName = $"M:{method.DeclaringType?.FullName}.{method.Name}";
                    if (method.GetParameters().Length > 0)
                    {
                        var paramTypes = string.Join(
                            ",",
                            method
                                .GetParameters()
                                .Select(p => p.ParameterType.FullName?.Replace("+", "."))
                        );
                        memberName += $"({paramTypes})";
                    }

                    var node = xmlDoc.SelectSingleNode($"//member[@name='{memberName}']/summary");
                    if (node != null)
                    {
                        return node.InnerText.Trim();
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}
