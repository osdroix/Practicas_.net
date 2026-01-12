using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public bool IsCompleted { get; internal set; }
        [Required(ErrorMessage = "CategoryId es requerido.")]
        public int CategoryId { get; set; }
    }
}
