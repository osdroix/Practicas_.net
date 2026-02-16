using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    // ----------------------------------------------------------------------------------
    // Date: 2026-02-11
    // Author: [Usuario]
    // Description: Implementación del servicio de tareas (TaskService).
    // Esta clase encapsula toda la lógica de negocio relacionada con las tareas,
    // manejando la comunicación con el cliente API (ITaskApiClient) y el control de errores.
    // Se utiliza el patrón ServiceResult para devolver resultados estandarizados al controlador.
    // ----------------------------------------------------------------------------------
    public class TaskService : ITaskService
    {
        private readonly ITaskApiClient _client;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskApiClient client, ILogger<TaskService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> GetAllTasksAsync(int page = 1, int pageSize = 10)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Obtención paginada de tareas con manejo de excepciones centralizado.
            // ----------------------------------------------------------------------------------
            try
            {
                var result = await _client.GetTasksAsync(page, pageSize);
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks");
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Fail("Error al obtener las tareas: " + ex.Message);
            }
        }

        public async Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> SearchTasksAsync(TaskSearchViewModel model)
        {
            try
            {
                // ----------------------------------------------------------------------------------
                // Date: 2026-02-11
                // Author: [Usuario]
                // Description: Lógica de negocio migrada desde el controlador.
                // Se asegura que la página mínima sea 1 antes de llamar a la API.
                // ----------------------------------------------------------------------------------
                
                // Original Code (in controller):
                // if (model.Page == 0) model.Page = 1;

                if (model.Page == 0)
                    model.Page = 1;

                var result = await _client.SearchTasksAsync(model);
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tasks");
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Fail("Error al buscar tareas: " + ex.Message);
            }
        }

        public async Task<ServiceResult> CreateTaskAsync(CreateTaskViewModel model)
        {
            try
            {
                await _client.CreateTaskAsync(model);
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return ServiceResult.Fail("Error al crear la tarea: " + ex.Message);
            }
        }

        public async Task<ServiceResult<EditTaskViewModel>> GetTaskForEditAsync(int id)
        {
            try
            {
                var model = await _client.GetTaskByIdAsync(id);
                if (model == null)
                    return ServiceResult<EditTaskViewModel>.Fail("Tarea no encontrada");
                
                return ServiceResult<EditTaskViewModel>.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task for edit");
                return ServiceResult<EditTaskViewModel>.Fail("Error al obtener la tarea: " + ex.Message);
            }
        }

        public async Task<ServiceResult> UpdateTaskAsync(EditTaskViewModel model)
        {
            try
            {
                await _client.UpdateTaskAsync(model);
                return ServiceResult.Ok("La tarea fue actualizada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                return ServiceResult.Fail("Ocurrió un error: " + ex.Message);
            }
        }

        public async Task<ServiceResult> DeleteTaskAsync(int id)
        {
            try
            {
                await _client.DeleteTaskAsync(id);
                return ServiceResult.Ok("La tarea fue eliminada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task");
                return ServiceResult.Fail("No se pudo eliminar la tarea: " + ex.Message);
            }
        }

        public async Task<ServiceResult<TaskViewModel>> GetTaskDetailsAsync(int id)
        {
            try
            {
                var task = await _client.GetTaskDetailAsync(id);
                if (task == null)
                    return ServiceResult<TaskViewModel>.Fail("La tarea no existe.");

                return ServiceResult<TaskViewModel>.Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task details");
                return ServiceResult<TaskViewModel>.Fail("Error al obtener detalles: " + ex.Message);
            }
        }

        public async Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> AdvancedSearchAsync(TaskSearchViewModel filters)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Método para manejar la búsqueda avanzada, utilizado por la vista unificada.
            // ----------------------------------------------------------------------------------
            try
            {
                var result = await _client.AdvancedSearchAsync(filters);
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
                return ServiceResult<PagedResultViewModel<TaskViewModel>>.Fail("Error en búsqueda avanzada: " + ex.Message);
            }
        }

        public async Task<ServiceResult<List<TaskViewModel>>> GetTasksForAjaxAsync(string text)
        {
            // ----------------------------------------------------------------------------------
            // Date: 2026-02-11
            // Author: [Usuario]
            // Description: Servicio específico para la búsqueda AJAX.
            // Establece un tamaño de página fijo (20) para resultados rápidos.
            // ----------------------------------------------------------------------------------
            try
            {
                var result = await _client.SearchTasksAsync(new TaskSearchViewModel
                {
                    Text = text,
                    Page = 1,
                    PageSize = 20
                });
                return ServiceResult<List<TaskViewModel>>.Ok(result.Items.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ajax search");
                return ServiceResult<List<TaskViewModel>>.Fail("Error en búsqueda ajax: " + ex.Message);
            }
        }
    }
}
