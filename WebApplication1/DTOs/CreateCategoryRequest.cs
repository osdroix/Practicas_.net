using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Name es requerido.")]
        [MaxLength(100, ErrorMessage = "Name no puede superar 100 caracteres.")]
        public string Name { get; set; }
    }
}
