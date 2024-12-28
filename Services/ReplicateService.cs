using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace backend.Services;
public class ReplicateService {
    private readonly HttpClient _client;
    private const string MODEL_VERSION = "39ed52f2a78e934b3ba6e2a89f5b1c712de7dfea535525255b1aa35c5565e08b";
    private const string BASE_URL = "https://api.replicate.com/v1/predictions";

    public ReplicateService(IConfiguration configuration) {
        var apiKey = configuration["Replicate:ApiKey"]
            ?? throw new InvalidOperationException("Replicate API key is missing");

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
    }

    public async Task<List<string>> GenerateHairStyles(string imageData, string prompt) {
        var results = new List<string>();

        for (int i = 0; i < 3; i++) {
            try {
                var imageUrl = imageData.StartsWith("data:image")
                    ? imageData
                    : $"data:image/jpeg;base64,{imageData}";

                var requestBody = JsonSerializer.Serialize(new {
                    version = MODEL_VERSION,
                    input = new {
                        prompt = prompt,
                        image = imageUrl,
                        num_inference_steps = 50,
                        guidance_scale = 7.5,
                        negative_prompt = "unrealistic, cartoon, anime, drawing"
                    }
                });

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(BASE_URL, content);

                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Invalid request: {errorContent}");
                }

                response.EnsureSuccessStatusCode();

                var predictionJson = await response.Content.ReadAsStringAsync();
                var prediction = JsonSerializer.Deserialize<JsonElement>(predictionJson);
                var predictionId = prediction.GetProperty("id").GetString();

                while (true) {
                    await Task.Delay(1000);
                    response = await _client.GetAsync($"{BASE_URL}/{predictionId}");
                    response.EnsureSuccessStatusCode();

                    var statusJson = await response.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<JsonElement>(statusJson);

                    if (status.GetProperty("status").GetString() == "succeeded") {
                        var output = status.GetProperty("output")[0].GetString();
                        if (output != null)
                            results.Add(output);
                        break;
                    }
                }
            } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) {
                throw new InvalidOperationException("Invalid image format or request parameters");
            }
        }

        return results;
    }
}
