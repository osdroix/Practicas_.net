using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.DTOs;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaksItemsController : ControllerBase
    {
        private readonly DBContext _context;

        public TaksItemsController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaksItem>>> Get()
        {
            var tasks = await _context.TaksItems // Access the TaksItems DbSet from the database context
            .Select(t => new TaskItemResponse
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted
            })
            .ToListAsync();

                    return Ok(tasks);// retorna la lista de tareas que se mapean a TaskItemResponse
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaksItem>> GetById(int id)
        {
            var task = await _context.TaksItems.FindAsync(id);

            if (task == null)
                return NotFound();

            var dto = new TaskItemResponse
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = task.IsCompleted
            };

            return Ok(dto);
        }
        [HttpPost]
        public async Task<ActionResult<TaksItem>> Create([FromBody] CreateTaskRequest request)
        {
            if (request == null)
                return BadRequest("Body requerido.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title es requerido.");

            var entity = new TaksItem
            {
                Title = request.Title.Trim(),
                IsCompleted = false
            };

            _context.TaksItems.Add(entity);
            int contadorcambios = await _context.SaveChangesAsync();

            var dto = new TaskItemResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                IsCompleted = entity.IsCompleted
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        //Tolist
        //updete
        [HttpPost]
        [Route("update/{id}")]
        public async Task<ActionResult<TaskItemResponse>> Update(int id, [FromBody] UpdateTaksRequest request)
            // [FromBody] → Atributo que indica que este parámetro se obtiene del cuerpo de la solicitud 
        {
            if (request == null)
                return BadRequest("Body requerido.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title es requerido.");

            var task = await _context.TaksItems.FindAsync(id);

            if (task == null)
                return NotFound();

            // Modificar solo estos campos:
            task.Title = request.Title.Trim();
            // modificar el estado 
            if (request.IsCompleted.HasValue)
            {
                task.IsCompleted = request.IsCompleted.Value;
            }

            await _context.SaveChangesAsync();

            var dto = new TaskItemResponse
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = (bool)task.IsCompleted
            };

            return Ok(dto);
        }
        //delete se boloquean
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaksItems.FindAsync(id);
            if (task == null) return NotFound();

            _context.TaksItems.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> Search(
            [FromQuery] SearchFilter request)
        { 
            var query = _context.TaksItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.text))
                query = query.Where(t => t.Title.Contains(request.text));

            if (request.completed.HasValue)
                query = query.Where(t => t.IsCompleted == request.completed);

            if (request.step.HasValue)
                query = query.Where(t => t.Step == request.step);

            query = request.orderBy switch
            {
                "title" => query.OrderBy(t => t.Title),
                "title_desc" => query.OrderByDescending(t => t.Title),
                "date" => query.OrderBy(t => t.CreatedAt),
                "date_desc" => query.OrderByDescending(t => t.CreatedAt),
                "step" => query.OrderBy(t => t.Step),
                "step_desc" => query.OrderByDescending(t => t.Step),
                _ => query.OrderBy(t => t.Id)
            };

            var results = await query
                .Select(t => new TaskQueryResultDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }
        [HttpGet("paged")] public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10) { var query = _context.TaksItems.OrderBy(t => t.Id).Skip((page - 1) * pageSize).Take(pageSize); var result = await query.Select(t => new TaskQueryResultDto { Id = t.Id, Title = t.Title, IsCompleted = t.IsCompleted, Step = t.Step, CreatedAt = t.CreatedAt }).ToListAsync(); return Ok(result); }
    }
    /*los datos no son bueno que terminen expuestos directamente, Data Transfer Object,
     esto solo es para usar datos de forma directa y sin llamar todo el pull de estos.*/
}
