using Microsoft.AspNetCore.Builder;
using TaskManager.Web.Middleware;

namespace TaskManager.Web.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMvcGlobalErrorHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MvcGlobalErrorHandlerMiddleware>();
        }
    }
}