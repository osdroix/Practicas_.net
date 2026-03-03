using Microsoft.AspNetCore.Http;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    public interface ICategoryService
    {
        Task<ServiceResult<string>> ImportCategoriesFromExcelAsync(IFormFile file);
        Task<ServiceResult<List<CategoryItemViewModel>>> GetCategoriesAsync();
    }
}
