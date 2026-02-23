﻿using Microsoft.AspNetCore.Http;
using TaskManager.DTOs;
using TaskManager.DTOs.TaskDto;

namespace TaskManager.Interfaces.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskItemResponse>> GetAllAsync();
        Task<ServiceResult<TaskImportResultDto>> ImportFromExcelAsync(IFormFile file);
        Task<ServiceResult<TaskItemResponse>> GetByIdAsync(int id);
        Task<ServiceResult<TaskItemResponse>> CreateAsync(CreateTaskRequest request);
        Task<ServiceResult> UpdateAsync(int id, UpdateTaskRequest request);
        Task<ServiceResult> DeleteAsync(int id);
        Task<IEnumerable<TaskQueryResultDto>> SearchAsync(SearchFilter request);
        Task<IEnumerable<TaskQueryResultDto>> GetPagedAsync(PaginationDto pagination);
        Task<IEnumerable<TaskWithCategoryDto>> GetWithCategoryAsync();
        Task<ServiceResult<PagedResultDto<TaskWithCategoryDto>>> AdvancedSearchAsync(
            string? text,
            bool? completed,
            int? step,
            int? categoryId,
            string? categoryName,
            int page,
            int pageSize
         );
        Task<List<TaskAjaxSearchDto>> AjaxSearchAsync(string? text);
    }
}
