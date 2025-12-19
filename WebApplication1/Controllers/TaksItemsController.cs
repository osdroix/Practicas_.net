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

    }
    /*los datos no son bueno que terminen expuestos directamente, Data Transfer Object,
     esto solo es para usar datos de forma directa y sin llamar todo el pull de estos.*/
}