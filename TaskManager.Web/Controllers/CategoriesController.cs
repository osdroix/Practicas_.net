using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
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

            var result = await _categoryService.ImportCategoriesFromExcelAsync(file);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
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
            var result = await _categoryService.GetCategoriesAsync();

            if (result.Success)
            {
                return Json(result.Data);
            }

            var statusCode = result.StatusCode ?? 500;
            if (string.IsNullOrWhiteSpace(result.Detail))
            {
                return StatusCode(statusCode, new { message = result.Message });
            }

            return StatusCode(statusCode, new { message = result.Message, detail = result.Detail });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesList()
        {
            var result = await _categoryService.GetCategoriesAsync();

            if (result.Success)
            {
                return Json(result.Data);
            }

            var statusCode = result.StatusCode ?? 500;
            if (string.IsNullOrWhiteSpace(result.Detail))
            {
                return StatusCode(statusCode, new { message = result.Message });
            }

            return StatusCode(statusCode, new { message = result.Message, detail = result.Detail });
        }
    }
}
