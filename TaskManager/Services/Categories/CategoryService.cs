using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskManager.Context;
using TaskManager.DTOs;
using TaskManager.DTOs.CategoryDto;
using TaskManager.Interfaces.Categories;
using TaskManager.Models;

namespace TaskManager.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            var result = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<CategoryDto>>.Ok(result);
        }

        public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request)
        {
            var entity = new Category
            {
                Name = request.Name.Trim()
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();

            var dto = new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name
            };

            return ServiceResult<CategoryDto>.Created(dto);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return ServiceResult<CategoryDto>.Failure("Not Found", 404);

            var dto = new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name
            };

            return ServiceResult<CategoryDto>.Ok(dto);
        }

        public async Task<ServiceResult<CategoryImportResultDto>> ImportFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<CategoryImportResultDto>.Failure("No se recibió ningún archivo o está vacío.");

            var categories = new List<Category>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Nos aseguramos de ir al inicio

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First(); // Tomamos la primera hoja
                    var rows = worksheet.RangeUsed().RowsUsed();

                    bool isHeader = true;

                    foreach (var row in rows)
                    {
                        if (isHeader)
                        {
                            // Saltar la fila de cabeceras
                            isHeader = false;
                            continue;
                        }

                        var name = row.Cell(1).GetString();      // Columna A
                        var code = row.Cell(2).GetString();      // Columna B
                        var isActiveCell = row.Cell(3).GetString(); // Columna C

                        bool isActive = true;
                        if (!string.IsNullOrWhiteSpace(isActiveCell))
                        {
                            // TRUE/FALSE, 1/0, Sí/No... aquí se puede refinar
                            bool.TryParse(isActiveCell, out isActive);
                        }

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            // Se puede decidir saltar o romper
                            continue;
                        }

                        var category = new Category
                        {
                            Name = name.Trim(),
                            Code = code?.Trim(),
                            IsActive = isActive
                        };

                        categories.Add(category);
                    }
                }
            }
            // Opcional: filtrar duplicados por Name en la misma importación
            categories = categories
                .GroupBy(c => c.Name.ToLower())
                .Select(g => g.First())
                .ToList();

            // Opcional: evitar insertar categorías que ya existan en la BD
            var existingNames = _context.Categories
                .Select(c => c.Name.ToLower())
                .ToHashSet();

            var newCategories = categories
                .Where(c => !existingNames.Contains(c.Name.ToLower()))
                .ToList();

            // Guardar en base de datos
            _context.Categories.AddRange(newCategories);
            await _context.SaveChangesAsync();

            return ServiceResult<CategoryImportResultDto>.Ok(new CategoryImportResultDto
            {
                Message = $"Se importaron {categories.Count} categorías.",
                Count = categories.Count
            });
        }
    }
}
