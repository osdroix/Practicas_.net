using System.Text.Json.Serialization;

namespace TaskManager.Web.Models
{
    public class CategoryItemViewModel
    {
        // Id normalizado para usarlo en los select del frontend
        public int Id { get; set; }
        // Nombre normalizado para mostrar en los select
        public string Name { get; set; }

        // Alias para soportar respuestas con categoryId/categoryName desde la API
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        // Alias para soportar respuestas con categoryId/categoryName desde la API
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }
    }
}
