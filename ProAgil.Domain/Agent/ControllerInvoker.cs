using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Serviço responsável por invocar controllers via reflection
    /// </summary>
    public interface IControllerInvoker
    {
        Task<object> InvokeController(
            string controllerName,
            string actionName,
            object[] methodParams
        );

        (Type ControllerType, MethodInfo Method) FindControllerAndMethod(
            string controllerName,
            string actionName
        );
    }

    public class ControllerInvoker : IControllerInvoker
    {
        private readonly IServiceProvider _serviceProvider;

        public ControllerInvoker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public (Type ControllerType, MethodInfo Method) FindControllerAndMethod(
            string controllerName,
            string actionName
        )
        {
            var fullControllerName = controllerName.EndsWith("Controller")
                ? controllerName
                : controllerName + "Controller";

            var webApiAssembly = Assembly.Load("Globo.Giro.UI.WebApi");
            var controllerType = webApiAssembly
                .GetTypes()
                .FirstOrDefault(t =>
                    t.Name.Equals(fullControllerName, StringComparison.OrdinalIgnoreCase)
                    && typeof(ControllerBase).IsAssignableFrom(t)
                );

            if (controllerType == null)
            {
                throw new InvalidOperationException(
                    $"Controller '{fullControllerName}' não encontrado."
                );
            }

            var method = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase)
                    && (
                        m.GetCustomAttributes<HttpPostAttribute>().Any()
                        || m.GetCustomAttributes<HttpGetAttribute>().Any()
                        || m.GetCustomAttributes<HttpPutAttribute>().Any()
                        || m.GetCustomAttributes<HttpDeleteAttribute>().Any()
                    )
                );

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Action '{actionName}' não encontrada no controller '{fullControllerName}'."
                );
            }

            return (controllerType, method);
        }

        public async Task<object> InvokeController(
            string controllerName,
            string actionName,
            object[] methodParams
        )
        {
            var (controllerType, method) = FindControllerAndMethod(controllerName, actionName);

            var controllerInstance = ActivatorUtilities.CreateInstance(
                _serviceProvider,
                controllerType
            );
            var result = method.Invoke(controllerInstance, methodParams);

            // Processar resultado
            if (result is Task taskResult)
            {
                await taskResult.ConfigureAwait(false);
                var resultProperty = taskResult.GetType().GetProperty("Result");
                if (resultProperty != null)
                {
                    var actionResult = resultProperty.GetValue(taskResult);
                    if (actionResult is ObjectResult objectResult)
                    {
                        return objectResult.Value!;
                    }
                    return actionResult!;
                }
            }

            return result!;
        }
    }
}
