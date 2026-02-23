using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    public class CategoryApiClient : ICategoryApiClient
    {
        private readonly HttpClient _httpClient;

        public CategoryApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> ImportCategoriesFromExcelAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/categories/import-excel", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al importar categorías. Respuesta API: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<ImportCategoriesResult>();

            if (result == null || string.IsNullOrWhiteSpace(result.Message))
            {
                return "Importación realizada correctamente.";
            }

            return result.Message +
                   (result.Duplicadas > 0
                        ? $" ({result.Duplicadas} filas duplicadas no se importaron.)"
                        : string.Empty);
        }

        public async Task<List<CategoryItemViewModel>> GetCategoriesAsync()
        {
            // Se obtiene el JSON crudo para soportar múltiples formatos de respuesta
            var response = await _httpClient.GetAsync("/api/categories");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CategoryItemViewModel>();
            }

            // Normaliza distintas estructuras (array directo, data/items/categories)
            return ParseCategories(json);
        }

        private static List<CategoryItemViewModel> ParseCategories(string json)
        {
            // Parser flexible para aceptar variaciones de nombres de propiedades
            var result = new List<CategoryItemViewModel>();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            IEnumerable<JsonElement> items = Enumerable.Empty<JsonElement>();

            if (root.ValueKind == JsonValueKind.Array)
            {
                items = root.EnumerateArray();
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (TryGetPropertyIgnoreCase(root, "items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                {
                    items = itemsElement.EnumerateArray();
                }
                else if (TryGetPropertyIgnoreCase(root, "data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                {
                    items = dataElement.EnumerateArray();
                }
                else if (TryGetPropertyIgnoreCase(root, "categories", out var categoriesElement) && categoriesElement.ValueKind == JsonValueKind.Array)
                {
                    items = categoriesElement.EnumerateArray();
                }
            }

            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                // Soporta id/categoryId y name/categoryName
                var id = GetInt(item, "id", "categoryId");
                var name = GetString(item, "name", "categoryName");

                result.Add(new CategoryItemViewModel
                {
                    Id = id,
                    CategoryId = id,
                    Name = name ?? string.Empty,
                    CategoryName = name ?? string.Empty
                });
            }

            return result;
        }

        private static int GetInt(JsonElement element, params string[] names)
        {
            // Busca una propiedad numérica o string numérico por nombre (case-insensitive)
            foreach (var name in names)
            {
                if (TryGetPropertyIgnoreCase(element, name, out var value))
                {
                    if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                    {
                        return number;
                    }

                    if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            return 0;
        }

        private static string? GetString(JsonElement element, params string[] names)
        {
            // Busca una propiedad string por nombre (case-insensitive)
            foreach (var name in names)
            {
                if (TryGetPropertyIgnoreCase(element, name, out var value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString();
                    }
                }
            }

            return null;
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            // Comparación de nombre sin distinguir mayúsculas/minúsculas
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
