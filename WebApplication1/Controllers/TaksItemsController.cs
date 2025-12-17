using Microsoft.AspNetCore.Mvc;
using System;
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
        public List<TaksItem> Get()
        {
            return _context.TaksItems.ToList();
        }
    }
}