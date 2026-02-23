﻿using Microsoft.AspNetCore.Http;
using TaskManager.DTOs;
using TaskManager.DTOs.CategoryDto;

namespace TaskManager.Interfaces.Categories
{
    public interface ICategoryService
    {
        Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request);
        Task<ServiceResult<CategoryDto>> GetByIdAsync(int id);
        Task<ServiceResult<CategoryImportResultDto>> ImportFromExcelAsync(IFormFile file);
    }
}
