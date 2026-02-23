﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskManager.Context;
using TaskManager.DTOs.TaskDto;
using TaskManager.Interfaces.Tasks;
using TaskManager.Models;
using TaskManager.Utilities.Exceptions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITaskService _taskService;
        public TasksController(AppDbContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> Get()
        {
            var tasks = await _taskService.GetAllAsync();
            return Ok(tasks);
        }
        [HttpPost("import-tasks-excel")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            var result = await _taskService.ImportFromExcelAsync(file);
            if (!result.Success) return BadRequest(result.ErrorMessage);
            return Ok(result.Data);
        }

        // Convierte un string a booleano reconociendo múltiples formatos
        private bool ConvertToBoolean(string v)
        {
            // Si está vacío o es null, retorna false
            if (string.IsNullOrWhiteSpace(v)) return false;

            // Normaliza el texto a mayúsculas sin espacios
            v = v.Trim().ToUpper();

            // Retorna true si coincide con alguno de estos valores
            return v == "TRUE" || v == "VERDADERO" || v == "1" || v == "SÍ" || v == "YES";
        }
        // Retorna un registro
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            var result = await _taskService.GetByIdAsync(id);
            if (!result.Success) return NotFound();
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskRequest request)
        {
            var result = await _taskService.CreateAsync(request);
            if (!result.Success) return BadRequest(result.ErrorMessage);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        // Update
        // Delete: no se borra, solo se pone una bandera como True y solo me regresa los que tengan ese registro.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            var result = await _taskService.UpdateAsync(id, request);
            if (!result.Success)
            {
                if (result.StatusCode == 404) return NotFound();
                return BadRequest(result.ErrorMessage);
            }
            return NoContent(); // 204
        }

        // Delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteAsync(id);
            if (!result.Success) return NotFound();
            return NoContent();
        }

        // Search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> Search(
            [FromQuery] SearchFilter request
            )
        {
            var results = await _taskService.SearchAsync(request);
            return Ok(results);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> GetPaged(
            [FromQuery] PaginationDto pagination // Dto
            )
        {
            var result = await _taskService.GetPagedAsync(pagination);
            return Ok(result);
        }

        [HttpGet("with-category")]
        public async Task<ActionResult<IEnumerable<TaskWithCategoryDto>>> GetWithCategory()
        {
            var result = await _taskService.GetWithCategoryAsync();
            return Ok(result);
        }


        // Busqueda avanzada
        [HttpGet("advanced-search")]
        public async Task<ActionResult<PagedResultDto<TaskWithCategoryDto>>> AdvancedSearch(
    [FromQuery] string? text,
    [FromQuery] bool? completed,
    [FromQuery] int? step,
    [FromQuery] int? categoryId,
    [FromQuery] string? categoryName,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10
)
        {
            //throw new BusinessException("Hola que hace");
            var result = await _taskService.AdvancedSearchAsync(
                text, completed, step, categoryId, categoryName, page, pageSize);

            if (!result.Success) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }
        [HttpGet("ajax-search")]
        public async Task<IActionResult> AjaxSearch([FromQuery] string? text)
        {
            var results = await _taskService.AjaxSearchAsync(text);
            return Ok(results);
        }
    }


}
