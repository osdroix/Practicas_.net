using Microsoft.AspNetCore.Http;

namespace TaskManager.Web.Services
{
    public interface ICategoryApiClient
    {
        Task<string> ImportCategoriesFromExcelAsync(IFormFile file);
        Task<List<TaskManager.Web.Models.CategoryItemViewModel>> GetCategoriesAsync();
    }
}
