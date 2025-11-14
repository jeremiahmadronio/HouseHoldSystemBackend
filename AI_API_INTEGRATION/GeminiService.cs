using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.AI_API_INTEGRATION
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(IConfiguration configuration, HttpClient? client = null)
        {
            _httpClient = client ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(40);

            _apiKey = configuration["Gemini:ApiKey"]
                ?? throw new Exception("Gemini:ApiKey missing");
        }

        public async Task<string> GenerateTextAsync(string prompt, string model = "gemini-2.0-flash")
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            // 🔥 CORRECT FORMAT REQUIRED BY GEMINI
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PostAsync(url, content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini error {response.StatusCode}: {raw}");

            return ExtractText(raw);
        }

        // 🔥 Extract JSON-only text safely
        private string ExtractText(string raw)
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates)
                && candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];

                if (first.TryGetProperty("content", out var content)
                    && content.TryGetProperty("parts", out var parts)
                    && parts.GetArrayLength() > 0)
                {
                    if (parts[0].TryGetProperty("text", out var textNode))
                        return textNode.GetString() ?? raw;
                }
            }

            return raw;
        }
    }
}
