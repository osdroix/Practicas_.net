using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Context;
using TaskManager.DTOs.CategoryDto;
using TaskManager.Interfaces.Categories;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(result.Data);
        }

        // Post
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateAsync(request);
            if (!result.Success) return BadRequest(result.ErrorMessage);

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        // obtener categoría en especifico
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.Success) return NotFound();

            return Ok(result.Data);
        }
        [HttpPost("import-excel")] // los archivos se envían por POST
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            var result = await _categoryService.ImportFromExcelAsync(file);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new
            {
                Message = result.Data.Message
            });
        }

    }
}
