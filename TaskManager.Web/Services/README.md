# Documentación de la Capa de Servicios

Esta documentación describe la nueva arquitectura de servicios implementada para desacoplar el controlador `TasksController` de la lógica de negocio y el acceso a datos.

## Estructura

La lógica de negocio se ha migrado a una capa de servicios, introduciendo los siguientes componentes:

### 1. ITaskService (Interfaz)
Ubicación: `TaskManager.Web/Interfaces/Task/ITaskService.cs`

Define el contrato para las operaciones relacionadas con las tareas. Abstrae la implementación subyacente (API, base de datos, etc.) del controlador.

Métodos principales:
- `GetAllTasksAsync`: Obtiene tareas paginadas.
- `SearchTasksAsync`: Realiza búsquedas con filtros.
- `CreateTaskAsync`: Crea una nueva tarea.
- `GetTaskForEditAsync`: Obtiene una tarea para edición.
- `UpdateTaskAsync`: Actualiza una tarea existente.
- `DeleteTaskAsync`: Elimina una tarea.
- `GetTaskDetailsAsync`: Obtiene los detalles de una tarea.
- `AdvancedSearchAsync`: Búsqueda avanzada.
- `GetTasksForAjaxAsync`: Búsqueda rápida para componentes Ajax.

### 2. TaskService (Implementación)
Ubicación: `TaskManager.Web/Services/Task/TaskService.cs`

Implementa `ITaskService`. Sus responsabilidades incluyen:
- **Orquestación**: Llama al `ITaskApiClient` para comunicarse con el backend.
- **Manejo de Excepciones**: Captura excepciones de la capa inferior y las envuelve en un resultado controlado (`ServiceResult`).
- **Lógica de Negocio**: Contiene reglas de negocio como valores por defecto para paginación.

### 3. ServiceResult (Patrón de Resultado)
Ubicación: `TaskManager.Web/Models/ServiceResult.cs`

Clase genérica utilizada para estandarizar las respuestas de los servicios.
- `Success`: Indica si la operación fue exitosa.
- `Message`: Mensaje de error o éxito.
- `Data`: Datos retornados por el servicio (en caso de éxito).

## Migración del Controlador

El `TasksController` ha sido refactorizado para:
- Depender de `ITaskService` en lugar de `ITaskApiClient`.
- Eliminar bloques `try-catch` y lógica de validación de negocio.
- Usar `ServiceResult` para determinar si mostrar la vista de éxito o error.

### Ejemplo de Flujo

**Antes (Controlador):**
```csharp
try {
    await _client.DeleteTaskAsync(id);
    TempData["Success"] = "...";
} catch (Exception ex) {
    TempData["Error"] = ex.Message;
}
```

**Ahora (Controlador):**
```csharp
var result = await _service.DeleteTaskAsync(id);
if (result.Success)
    TempData["Success"] = result.Message;
else
    TempData["Error"] = result.Message;
```

## Registro de Servicios

Se ha registrado el servicio en el contenedor de inyección de dependencias en `Program.cs` (vía `ServiceCollectionExtensions`):

```csharp
builder.Services.AddScoped<ITaskService, TaskService>();
```
