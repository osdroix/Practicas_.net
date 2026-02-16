using TaskManager.Web.Services;

namespace TaskManager.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiClients(this IServiceCollection services)
        {
            services.AddHttpClient<ITaskApiClient, TaskApiClient>();
            services.AddHttpClient<ICategoryApiClient, CategoryApiClient>();

            return services;
        }

        // ----------------------------------------------------------------------------------
        // Date: 2026-02-11
        // Author: [Usuario]
        // Description: Método de extensión para agrupar el registro de servicios de negocio.
        // Se registra TaskService como Scoped para que su ciclo de vida coincida con la solicitud HTTP.
        // ----------------------------------------------------------------------------------
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<ITaskService, TaskService>();
            return services;
        }
    }
}
