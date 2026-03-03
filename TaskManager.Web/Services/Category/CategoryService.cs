using Microsoft.AspNetCore.Http;
using TaskManager.Web.Models;
using TaskManager.Web.Utilities.Exceptions;

namespace TaskManager.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryApiClient categoryApiClient, ILogger<CategoryService> logger)
        {
            _categoryApiClient = categoryApiClient;
            _logger = logger;
        }

        public async Task<ServiceResult<string>> ImportCategoriesFromExcelAsync(IFormFile file)
        {
            try
            {
                var message = await _categoryApiClient.ImportCategoriesFromExcelAsync(file);
                return ServiceResult<string>.Ok(message, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing categories");
                return ServiceResult<string>.Fail("Ocurrió un error al importar el archivo: " + ex.Message, 500, ex.Message);
            }
        }

        public async Task<ServiceResult<List<CategoryItemViewModel>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _categoryApiClient.GetCategoriesAsync();
                return ServiceResult<List<CategoryItemViewModel>>.Ok(categories);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ServiceResult<List<CategoryItemViewModel>>.Fail(ex.Message, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ServiceResult<List<CategoryItemViewModel>>.Fail("Error al obtener categorías.", 500, ex.Message);
            }
        }
    }
}
