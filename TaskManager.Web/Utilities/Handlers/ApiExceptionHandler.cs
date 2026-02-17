using System.Text.Json;
using TaskManager.Web.Utilities.Exceptions;

namespace TaskManager.Web.Http
{
    public class ApiExceptionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return response;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var msg = TryExtractMessage(content) ?? $"Error en la API ({(int)response.StatusCode}).";
            throw new ApiException(msg, (int)response.StatusCode);
        }

        private static string? TryExtractMessage(string content)
        {
            try
            {
                var err = JsonSerializer.Deserialize<ApiErrorResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Tu API manda el mensaje real en "detail"
                return err?.detail ?? err?.message;
            }
            catch
            {
                return null;
            }
        }

        private sealed class ApiErrorResponse
        {
            public string? message { get; set; }
            public string? detail { get; set; }
        }
    }
}