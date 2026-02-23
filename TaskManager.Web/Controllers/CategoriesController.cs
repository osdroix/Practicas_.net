using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Services;
using TaskManager.Web.Utilities.Exceptions;

namespace TaskManager.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryApiClient _categoryApiClient;

        public CategoriesController(ICategoryApiClient categoryApiClient)
        {
            _categoryApiClient = categoryApiClient;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo Excel.";
                return View();
            }

            try
            {
                var resultMessage = await _categoryApiClient.ImportCategoriesFromExcelAsync(file);
                TempData["Success"] = resultMessage;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al importar el archivo: " + ex.Message;
            }

            return View();
        }

        public IActionResult Index()
        {
            return View(); // Queda pendiente para otra clase
        }

        [HttpGet("api/categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                // Endpoint JSON para el frontend (select de categorías)
                var categories = await _categoryApiClient.GetCategoriesAsync();
                return Json(categories);
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categorías.", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesList()
        {
            try
            {
                // Fallback para cuando el endpoint /api/categories no esté disponible en el navegador
                var categories = await _categoryApiClient.GetCategoriesAsync();
                return Json(categories);
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categorías.", detail = ex.Message });
            }
        }
    }
}
