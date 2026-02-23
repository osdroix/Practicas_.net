﻿using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskManager.Context;
using TaskManager.DTOs;
using TaskManager.DTOs.TaskDto;
using TaskManager.Interfaces.Tasks;
using TaskManager.Models;

namespace TaskManager.Services.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItemResponse>> GetAllAsync()
        {
            var tasks = await _context.Tasks
                .Select(t => new TaskItemResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<ServiceResult<TaskImportResultDto>> ImportFromExcelAsync(IFormFile file)
        {
            // Valida que el archivo exista y tenga contenido
            if (file == null || file.Length == 0) return ServiceResult<TaskImportResultDto>.Failure("Archivo no válido.");

            // Lista para almacenar las tareas leídas del Excel
            var tasks = new List<TaskItem>();

            // Lista para almacenar los errores durante el procesamiento
            var errores = new List<string>();

            // Crea un stream en memoria para leer el archivo
            using (var stream = new MemoryStream())
            {
                // Copia el archivo al stream
                await file.CopyToAsync(stream);

                // Resetea la posición al inicio para poder leerlo
                stream.Position = 0;

                // Abre el archivo Excel con ClosedXML
                using (var workbook = new XLWorkbook(stream))
                {
                    // Obtiene la primera hoja del Excel
                    var worksheet = workbook.Worksheets.First();

                    // Obtiene todas las filas con datos, saltando la primera (encabezados)
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    // Contador de filas para reportar errores
                    int rowNumber = 1;

                    // Itera cada fila del Excel
                    foreach (var row in rows)
                    {
                        rowNumber++;
                        try
                        {
                            // Lee el título de la columna 2
                            var title = row.Cell(2).GetString();

                            // Si el título está vacío, salta esta fila
                            if (string.IsNullOrWhiteSpace(title)) continue;

                            // Crea una nueva tarea con los datos de la fila
                            tasks.Add(new TaskItem
                            {
                                Title = title.Trim(), // Elimina espacios en blanco
                                IsCompleted = ConvertToBoolean(row.Cell(3).GetString()), // Convierte a booleano
                                Step = int.TryParse(row.Cell(4).GetString(), out int s) ? s : 0, // Convierte a entero, default 0
                                CategoryId = int.TryParse(row.Cell(5).GetString(), out int c) ? c : null, // Convierte a entero nullable
                                IsDeleted = ConvertToBoolean(row.Cell(6).GetString()) // Convierte a booleano
                            });
                        }
                        catch (Exception ex)
                        {
                            // Captura errores y los agrega a la lista con el número de fila
                            errores.Add($"Fila {rowNumber}: {ex.Message}"); // Error en esta fila
                        }
                    }
                }
            }

            // Extrae los IDs de categoría de las tareas, eliminando nulos y duplicados
            var categoryIds = tasks
                .Where(t => t.CategoryId.HasValue) // el where filtra los nulos
                .Select(t => t.CategoryId.Value) // el select obtiene el valor
                .Distinct() // elimina duplicados
                .ToList(); // convierte a lista

            // Consulta la base de datos para obtener los IDs de categorías que realmente existen
            var existingCategoryIds = await _context.Categories // consulta a la tabla de categorias
                .Where(c => categoryIds.Contains(c.Id)) // filtra por los ids obtenidos del excel
                .Select(c => c.Id) // selecciona solo los ids
                .ToListAsync(); // ejecuta la consulta

            // Encuentra las categorías que no existen en la base de datos
            var invalidCategoryIds = categoryIds.Except(existingCategoryIds).ToList();

            // Agrega un error por cada categoría inválida
            foreach (var id in invalidCategoryIds) errores.Add($"Categoría {id} no existe.");

            // Obtiene todos los títulos de tareas existentes en minúsculas para comparación
            var existingTitles = await _context.Tasks
                .Select(t => t.Title.ToLower().Trim())
                .ToHashSetAsync();

            // Filtra las tareas: solo las que tienen categoría válida, sin duplicados en el Excel, y que no existan en la BD
            var newTasks = tasks
                .Where(t => t.CategoryId.HasValue && existingCategoryIds.Contains(t.CategoryId.Value)) // where categoria valida
                .GroupBy(t => t.Title.ToLower().Trim()) // agrupa por titulo
                .Select(g => g.First()) // selecciona el primero de cada grupo (elimina duplicados)
                .Where(t => !existingTitles.Contains(t.Title.ToLower().Trim())) // filtra los que no existen en la BD
                .ToList(); // convierte a lista

            // Si hay tareas nuevas, las agrega a la base de datos
            if (newTasks.Any())
            {
                _context.Tasks.AddRange(newTasks);
                await _context.SaveChangesAsync();
            }

            // Retorna la cantidad de tareas guardadas y los errores encontrados
            return ServiceResult<TaskImportResultDto>.Ok(new TaskImportResultDto { Guardados = newTasks.Count, Errores = errores });
        }

        private bool ConvertToBoolean(string v)
        {
            // Si está vacío o es null, retorna false
            if (string.IsNullOrWhiteSpace(v)) return false;

            // Normaliza el texto a mayúsculas sin espacios
            v = v.Trim().ToUpper();

            // Retorna true si coincide con alguno de estos valores
            return v == "TRUE" || v == "VERDADERO" || v == "1" || v == "SÍ" || v == "YES";
        }

        public async Task<ServiceResult<TaskItemResponse>> GetByIdAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return ServiceResult<TaskItemResponse>.Failure("Not Found", 404);

            var dto = new TaskItemResponse
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = task.IsCompleted
            };

            return ServiceResult<TaskItemResponse>.Ok(dto);
        }

        public async Task<ServiceResult<TaskItemResponse>> CreateAsync(CreateTaskRequest request)
        {
            if (request == null)
                return ServiceResult<TaskItemResponse>.Failure("Body requerido.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return ServiceResult<TaskItemResponse>.Failure("Title es requerido.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                return ServiceResult<TaskItemResponse>.Failure("CategoryId no existe.");

            var entity = new TaskItem
            {
                Title = request.Title.Trim(),
                IsCompleted = false,
                CategoryId = request.CategoryId
            };

            // registro
            _context.Tasks.Add(entity);
            int ContadorCambios = await _context.SaveChangesAsync();

            var dto = new TaskItemResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                IsCompleted = entity.IsCompleted
            };

            return ServiceResult<TaskItemResponse>.Created(dto);
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return ServiceResult.Failure("Not Found", 404);

            if (request == null) return ServiceResult.Failure("Body requerido.");
            if (string.IsNullOrWhiteSpace(request.Title)) return ServiceResult.Failure("Title es requerido.");

            task.Title = request.Title.Trim();
            if (request.IsCompleted.HasValue)
            {
                task.IsCompleted = request.IsCompleted.Value;
            }
            if (request.Step.HasValue)
            {
                task.Step = request.Step.Value;
            }
            if (request.CategoryId.HasValue)
            {
                task.CategoryId = request.CategoryId.Value;
            }

            await _context.SaveChangesAsync();

            return ServiceResult.NoContent();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return ServiceResult.Failure("Not Found", 404);

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return ServiceResult.NoContent();
        }

        public async Task<IEnumerable<TaskQueryResultDto>> SearchAsync(SearchFilter request)
        {
            var query = _context.Tasks.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(request.text))
                query = query.Where(t => t.Title.Contains(request.text));

            if (request.completed.HasValue)
                query = query.Where(t => t.IsCompleted == request.completed);

            if (request.step.HasValue)
                query = query.Where(t => t.Step == request.step);

            // Ordenamiento
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

            // Paginación
            query = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

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

            return results;
        }

        public async Task<IEnumerable<TaskQueryResultDto>> GetPagedAsync(PaginationDto pagination)
        {
            var query = _context.Tasks
                .OrderBy(t => t.Id)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize);

            var result = await query.Select(t => new TaskQueryResultDto
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                Step = t.Step,
                CreatedAt = t.CreatedAt
            })
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<TaskWithCategoryDto>> GetWithCategoryAsync()
        {
            var result = await _context.Tasks
                .Include(t => t.Category) // forma estandard de un JOIN
                .OrderBy(t => t.Id)
                .Select(t => new TaskWithCategoryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step,
                    CreatedAt = t.CreatedAt,
                    CategoryId = t.CategoryId ?? 0,
                    CategoryName = t.Category.Name
                })
                .ToListAsync();

            return result;
        }

        public async Task<ServiceResult<PagedResultDto<TaskWithCategoryDto>>> AdvancedSearchAsync(
            string? text,
            bool? completed,
            int? step,
            int? categoryId,
            string? categoryName,
            int page,
            int pageSize
        )
        {
            if (page <= 0) return ServiceResult<PagedResultDto<TaskWithCategoryDto>>.Failure("Page debe ser mayor a 0.");
            if (pageSize <= 0 || pageSize > 100) return ServiceResult<PagedResultDto<TaskWithCategoryDto>>.Failure("PageSize debe estar entre 1 y 100.");

            var query = _context.Tasks
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(text))
                query = query.Where(t => t.Title.Contains(text));

            if (completed.HasValue)
                query = query.Where(t => t.IsCompleted == completed);

            if (step.HasValue)
                query = query.Where(t => t.Step == step);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var name = categoryName.Trim();
                query = query.Where(t => t.Category.Name.Contains(name));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskWithCategoryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step,
                    CreatedAt = t.CreatedAt,
                    CategoryId = t.CategoryId ?? 0,
                    CategoryName = t.Category.Name
                })
                .ToListAsync();

            return ServiceResult<PagedResultDto<TaskWithCategoryDto>>.Ok(new PagedResultDto<TaskWithCategoryDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            });
        }

        public async Task<List<TaskAjaxSearchDto>> AjaxSearchAsync(string? text)
        {
            var query = _context.Tasks.AsQueryable(); // Consulta base

            if (!string.IsNullOrWhiteSpace(text))
                query = query.Where(t => t.Title.Contains(text));

            var results = await query
                .OrderBy(t => t.Id)
                .Take(50)
                .Select(t => new TaskAjaxSearchDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    // CategoryName = t.CategoryName,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step
                })
                .ToListAsync();

            return results;
        }
    }
}
