using Microsoft.Extensions.DependencyInjection;
using TaskManager.Interfaces.Tasks;
using TaskManager.Services.Tasks; // <-- implementación de ITaskService
using TaskManager.Interfaces.Categories;
using TaskManager.Services.Categories; // <-- implementación de ICategoryService

namespace TaskManager.Utilities.Configurations
{
    public static class ServiceConfiguration
    {

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ICategoryService, CategoryService>();
        }
    }
}