using System.ComponentModel.DataAnnotations;

namespace TaskManager.Web.Models
{
    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoryId { get; set; }

        [Range(1, 5, ErrorMessage = "El step debe estar entre 1 y 5")]
        public int Step { get; set; }

        public bool IsCompleted { get; set; }
    }
}
