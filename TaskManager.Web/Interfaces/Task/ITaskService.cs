using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    // ----------------------------------------------------------------------------------
    // Date: 2026-02-11
    // Author: [Usuario]
    // Description: Interfaz que define el contrato del servicio de tareas.
    // Provee una abstracción sobre la lógica de negocio, permitiendo que el controlador
    // desconozca los detalles de implementación (llamadas HTTP, validaciones, etc.).
    // ----------------------------------------------------------------------------------
    public interface ITaskService
    {
        Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> GetAllTasksAsync(int page = 1, int pageSize = 10);
        Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> SearchTasksAsync(TaskSearchViewModel model);
        Task<ServiceResult> CreateTaskAsync(CreateTaskViewModel model);
        Task<ServiceResult<EditTaskViewModel>> GetTaskForEditAsync(int id);
        Task<ServiceResult> UpdateTaskAsync(EditTaskViewModel model);
        Task<ServiceResult> DeleteTaskAsync(int id);
        Task<ServiceResult<TaskViewModel>> GetTaskDetailsAsync(int id);
        Task<ServiceResult<PagedResultViewModel<TaskViewModel>>> AdvancedSearchAsync(TaskSearchViewModel filters);
        Task<ServiceResult<List<TaskViewModel>>> GetTasksForAjaxAsync(string text);
    }
}
