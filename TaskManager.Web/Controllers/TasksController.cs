using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManager.Web.Models;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers
{
    public class TasksController : Controller
    {
        // ----------------------------------------------------------------------------------
        // Date: 2026-02-11
        // Description: Se cambió la dependencia de ITaskApiClient a ITaskService.
        // Esto permite desacoplar el controlador de la lógica de comunicación directa con la API
        // y delegar la lógica de negocio a la capa de servicios.
        // ----------------------------------------------------------------------------------

        // Original Code:
        private readonly ITaskApiClient _client;
        private readonly ITaskService _service;
        private readonly ICategoryApiClient _categoryApiClient;

        // Original Code:
        // public TasksController(ITaskApiClient client)
        // {
        //     _client = client;
        // }
        public TasksController(ITaskService service, ICategoryApiClient categoryApiClient)
        {
            _service = service;
            _categoryApiClient = categoryApiClient;
        }

        // ----------------------------------------------------------------------------------
        // Date: 2026-02-11
        // Description: Método Index modificado para soportar la vista unificada (Tabs).
        // Se cambió la firma para aceptar TaskSearchViewModel en lugar de parámetros sueltos (page, pageSize).
        // Se utiliza AdvancedSearchAsync para manejar todos los casos de filtrado desde la vista principal.
        // ----------------------------------------------------------------------------------

        // Original Code:
        // public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        // {
        //     var result = await _client.GetTasksAsync(page, pageSize);
        //     return View(result);
        // }
        public async Task<IActionResult> Index(TaskSearchViewModel model)
        {
            var result = await _service.AdvancedSearchAsync(model);

            // Validación de resultado usando el patrón ServiceResult en lugar de try-catch
            if (result.Success)
            {
                model.Result = result.Data;
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                model.Result = new PagedResultViewModel<TaskViewModel>();
            }

            return View(model);
        }

        public async Task<IActionResult> Search(TaskSearchViewModel model)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Description: Migración de lógica de negocio al servicio.
            // Se eliminó la validación manual de paginación (if model.Page == 0) ya que ahora se maneja en el servicio.
            // ----------------------------------------------------------------------------------

            // Original Code:
            // if (model.Page == 0) model.Page = 1;
            // model.Result = await _client.SearchTasksAsync(model);

            var result = await _service.SearchTasksAsync(model);

            if (result.Success)
            {
                model.Result = result.Data;
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                model.Result = new PagedResultViewModel<TaskViewModel>();
            }

            return View("Index1", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Precarga de categorías para el select en create
            ViewData["Categories"] = await GetCategoriesSafeAsync();
            return View(new CreateTaskViewModel());
        }
        // obtenemos los datos de las categorías
       /* [HttpGet]
        public async Task<IActionResult> GetCatogories()
        {
            var models = await _service.GetCategoriesAsync();
            return models;
        }
       */
        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            var isAjax = Request.Headers.TryGetValue("X-Requested-With", out var header)
                         && header == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    return BadRequest(new
                    {
                        message = "Validación fallida.",
                        errors = GetModelErrors(ModelState)
                    });
                }

                // Reenvía categorías si hay error de validación en create
                ViewData["Categories"] = await GetCategoriesSafeAsync();
                return View(model);
            }

            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Description: Reemplazo de llamada directa al cliente por el servicio.
            // El servicio encapsula la lógica de creación.
            // ----------------------------------------------------------------------------------

            // Original Code:
            // await _client.CreateTaskAsync(model);

            var result = await _service.CreateTaskAsync(model);

            if (!result.Success)
            {
                if (isAjax)
                {
                    return StatusCode(400, new { message = result.Message });
                }

                ModelState.AddModelError("", result.Message);
                // Reenvía categorías si hay error de negocio en create
                ViewData["Categories"] = await GetCategoriesSafeAsync();
                return View(model);
            }

            if (isAjax)
            {
                return Ok(new { message = "Tarea creada correctamente." });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Description: Uso de ServiceResult para manejar errores de obtención (ej. 404).
            // ----------------------------------------------------------------------------------

            // Original Code:
            // var model = await _client.GetTaskByIdAsync(id);
            // return View(model);

            var result = await _service.GetTaskForEditAsync(id);

            if (!result.Success)
            {
                return NotFound(); // Or redirect with error
            }

            // Precarga de categorías para el select en edit
            ViewData["Categories"] = await GetCategoriesSafeAsync();
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTaskViewModel model)
        {
            var isAjax = Request.Headers.TryGetValue("X-Requested-With", out var header)
                         && header == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    return BadRequest(new
                    {
                        message = "Validación fallida.",
                        errors = GetModelErrors(ModelState)
                    });
                }

                // Reenvía categorías si hay error de validación en edit
                ViewData["Categories"] = await GetCategoriesSafeAsync();
                return View(model);
            }

            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Description: Eliminación de bloque try-catch en el controlador.
            // El manejo de excepciones ahora reside en la capa de servicios, retornando un ServiceResult.
            // ----------------------------------------------------------------------------------

            // Original Code:
            // try {
            //     await _client.UpdateTaskAsync(model);
            //     TempData["Success"] = "...";
            // } catch (Exception ex) {
            //     ModelState.AddModelError("", "..." + ex.Message);
            // }

            var result = await _service.UpdateTaskAsync(model);

            if (result.Success)
            {
                if (isAjax)
                {
                    return Ok(new { message = result.Message });
                }

                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (isAjax)
                {
                    return StatusCode(400, new { message = result.Message });
                }

                ModelState.AddModelError("", result.Message);
                // Reenvía categorías si hay error de negocio en edit
                ViewData["Categories"] = await GetCategoriesSafeAsync();
                return View(model);
            }
        }

        private static Dictionary<string, string[]> GetModelErrors(ModelStateDictionary modelState)
        {
            return modelState
                .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Refactorización para usar ServiceResult en eliminación.
            // Se simplifica la lógica de decisión para mensajes de UI (TempData).
            // ----------------------------------------------------------------------------------

            var result = await _service.DeleteTaskAsync(id);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Migración a ITaskService.GetTaskDetailsAsync.
            // ----------------------------------------------------------------------------------

            // Original Code:
            // var task = await _client.GetTaskDetailAsync(id);
            // if (task == null) { ... }

            var result = await _service.GetTaskDetailsAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Index2(TaskSearchViewModel filters)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Migración a ITaskService.AdvancedSearchAsync.
            // ----------------------------------------------------------------------------------

            var result = await _service.AdvancedSearchAsync(filters);

            if (result.Success)
            {
                filters.Result = result.Data;
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                filters.Result = new PagedResultViewModel<TaskViewModel>();
            }

            return View(filters);
        }

        [HttpGet]
        public IActionResult AjaxDemo()
        {
            return View();
        }

        [HttpGet("api/tasks/ajax-search")]
        public async Task<IActionResult> AjaxSearch(string text)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Description: Uso de servicio para búsqueda AJAX.
            // Se garantiza que siempre se retorne una lista válida (empty list on null).
            // ----------------------------------------------------------------------------------

            // Original Code:
            // var result = await _client.SearchTasksAsync(new TaskSearchViewModel { ... });
            // return Json(result.Items);

            var result = await _service.GetTasksForAjaxAsync(text);
            return Json(result.Data ?? new List<TaskViewModel>());
        }
        [HttpGet]
        public async Task<IActionResult> LoadTablePartial(TaskSearchViewModel filters)
        {
            var result = await _service.AdvancedSearchAsync(filters);
            var items = result.Success ? result.Data?.Items ?? Enumerable.Empty<TaskViewModel>()
                                       : Enumerable.Empty<TaskViewModel>();
            return PartialView("_TaskTablePartial", items);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePartial()
        {
            var model = new EditTaskViewModel
            {
                Id = 0,
                Title = string.Empty,
                CategoryId = 0,
                Step = 1,
                IsCompleted = false
            };

            // Precarga de categorías para el select del modal (crear)
            ViewData["Categories"] = await GetCategoriesSafeAsync();
            return PartialView("_TaskFormPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(int id)
        {
            var result = await _service.GetTaskForEditAsync(id);

            if (!result.Success)
            {
                return NotFound();
            }

            // Precarga de categorías para el select del modal (editar)
            ViewData["Categories"] = await GetCategoriesSafeAsync();
            return PartialView("_TaskFormPartial", result.Data);
        }

        private async Task<List<CategoryItemViewModel>> GetCategoriesSafeAsync()
        {
            try
            {
                // Manejo seguro: nunca romper la vista si falla la API de categorías
                return await _categoryApiClient.GetCategoriesAsync();
            }
            catch
            {
                return new List<CategoryItemViewModel>();
            }
        }
    }
}
