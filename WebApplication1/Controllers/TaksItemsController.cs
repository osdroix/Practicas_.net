using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        //delete se boloquean
    }
    /*los datos no son bueno que terminen expuestos directamente, Data Transfer Object,
     esto solo es para usar datos de forma directa y sin llamar todo el pull de estos.*/
}