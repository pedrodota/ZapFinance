using System.Text.Json;
using System.Text.RegularExpressions;

namespace Core.Service.Application.Services;

public class GoogleVisionService : IGoogleVisionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleVisionService> _logger;

    public GoogleVisionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleVisionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> ExtractTextFromImageAsync(string base64Image)
    {
        try
        {
            var result = await AnalyzeReceiptAsync(base64Image);
            return result.ExtractedText ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair texto da imagem");
            return string.Empty;
        }
    }

    public async Task<GoogleVisionResult> AnalyzeReceiptAsync(string base64Image)
    {
        try
        {
            var apiKey = _configuration["GoogleVision:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Google Vision API Key não configurada");
                return new GoogleVisionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "API Key não configurada"
                };
            }

            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64Image },
                        features = new[]
                        {
                            new { type = "TEXT_DETECTION", maxResults = 1 }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API do Google Vision: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);

                return new GoogleVisionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Erro na API: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var visionResponse = JsonSerializer.Deserialize<GoogleVisionApiResponse>(responseContent);

            if (visionResponse?.responses?.FirstOrDefault()?.textAnnotations?.Any() != true)
            {
                return new GoogleVisionResult
                {
                    IsSuccessful = true,
                    ExtractedText = string.Empty,
                    ErrorMessage = "Nenhum texto detectado na imagem"
                };
            }

            var extractedText = visionResponse.responses![0].textAnnotations![0].description;

            return new GoogleVisionResult
            {
                IsSuccessful = true,
                ExtractedText = extractedText
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar recibo com Google Vision");
            return new GoogleVisionResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
// Classes para deserialização da resposta da API
internal class GoogleVisionApiResponse
{
    public GoogleVisionResponseItem[]? responses { get; set; }
}

internal class GoogleVisionResponseItem
{
    public TextAnnotation[]? textAnnotations { get; set; }
}

internal class TextAnnotation
{
    public string description { get; set; } = string.Empty;
}
