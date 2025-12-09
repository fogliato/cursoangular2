using System;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Serviço responsável por gerar documentação da API
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
            sb.AppendLine("# API Endpoints Disponíveis\n");

            // Usar o API Explorer para obter todos os endpoints com documentação Swagger
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
                    sb.AppendLine($"Rota: {route}");

                    // Obter documentação XML do método
                    var methodInfo = actionDescriptor.MethodInfo;
                    var xmlDoc = GetXmlDocumentation(methodInfo);
                    if (!string.IsNullOrEmpty(xmlDoc))
                    {
                        sb.AppendLine($"Descrição: {xmlDoc}");
                    }

                    // Listar parâmetros com suas descrições
                    if (api.ParameterDescriptions.Any())
                    {
                        sb.AppendLine("Parâmetros:");
                        foreach (var param in api.ParameterDescriptions)
                        {
                            var paramType = param.Type?.Name ?? "object";
                            var paramName = param.Name;
                            var source = param.Source.DisplayName;

                            sb.AppendLine($"  - {paramName} ({paramType}) [{source}]");

                            // Se for um objeto complexo, listar suas propriedades
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
                                    sb.AppendLine($"    Propriedades de {paramType}:");
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
                $"[ApiDocumentationService] Documentação completa gerada: {documentation.Length} caracteres, {groupedByController.Count()} controllers"
            );
            return documentation;
        }

        private string GetXmlDocumentation(System.Reflection.MethodInfo method)
        {
            try
            {
                // Tentar obter de [Description]
                var descAttr =
                    method
                        .GetCustomAttributes(false)
                        .FirstOrDefault(a => a.GetType().Name == "DescriptionAttribute")
                    as DescriptionAttribute;

                if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    return descAttr.Description;

                // Tentar obter das tags XML via reflection no assembly
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
