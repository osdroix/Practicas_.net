using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using TaskManager.Web.Utilities.Exceptions;

namespace TaskManager.Web.Middleware
{
    public class MvcGlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MvcGlobalErrorHandlerMiddleware> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly ITempDataProvider _tempDataProvider;

        public MvcGlobalErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<MvcGlobalErrorHandlerMiddleware> logger,
            ITempDataDictionaryFactory tempDataFactory,

            ITempDataProvider tempDataProvider) {
            _next = next;
            _logger = logger;
            _tempDataFactory = tempDataFactory;
              _tempDataProvider = tempDataProvider;
            }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                // Errores que vienen de la API (400, 404, etc.)
                _logger.LogWarning(ex, "Error de API capturado en MVC. StatusCode: {StatusCode}", ex.StatusCode);

                var TempData = _tempDataFactory.GetTempData(context);
                TempData["Error"] = ex.Message;
                // CLAVE: Guardar TempData manualmente (cookie o sesión)
                _tempDataProvider.SaveTempData(context, TempData);


                // Redirigimos a una página amigable (por ejemplo, el listado)
                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.Redirect("/Tasks/Index");
                }

            }
            catch (Exception ex)
            {
                // Errores inesperados del propio MVC
                _logger.LogError(ex, "Error inesperado en MVC.");

                var TempData = _tempDataFactory.GetTempData(context);
                TempData["Error"] = "Ocurrió un error inesperado. Intente de nuevo más tarde.";
                // Guardar TempData manualmente
                _tempDataProvider.SaveTempData(context, TempData);

                // Aquí puedes redirigir a una vista de error genérica
                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.Redirect("/Tasks/Index");
                }

            }
        }
    }
}