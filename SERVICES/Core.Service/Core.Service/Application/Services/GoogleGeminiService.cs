using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Service.Application.Services;

public class GoogleGeminiService : IGoogleGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleGeminiService> _logger;

    public GoogleGeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleGeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> AnalyzeReceiptTextAsync(string receiptText)
    {
        try
        {
            var prompt = $@"
Analise o seguinte texto de recibo e extraia as informações principais:

{receiptText}

Por favor, forneça as seguintes informações em formato JSON:
- valor_total (decimal)
- estabelecimento (string)
- data (string no formato DD/MM/YYYY)
- categoria (string)
- itens (array de objetos com nome e valor)
- numero_parcelas (int, se for parcelado, senão null)
- forma_pagamento (string se identificável)

Responda apenas com o JSON, sem texto adicional.";

            var response = await CallGeminiApiAsync(prompt);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar texto do recibo com Gemini");
            return string.Empty;
        }
    }

    public async Task<GeminiReceiptAnalysis> AnalyzeReceiptImageAsync(string base64Image)
    {
        try
        {
            var prompt = @"
Analise esta imagem de recibo e extraia todas as informações possíveis.
Forneça as informações em formato JSON com os seguintes campos:
- valor_total (decimal)
- estabelecimento (string)
- data (string no formato DD/MM/YYYY)
- categoria (string sugerida baseada no tipo de estabelecimento)
- itens (array de objetos com nome, quantidade e valor)
- moeda (string, ex: BRL, USD)
- forma_pagamento (string se identificável)
- numero_parcelas (int, se for parcelado, senão null)

Seja preciso na extração dos valores numéricos e datas.
Procure por informações de parcelamento como '2x', '3x de R$ 50,00', 'parcelado em X vezes', etc.
Responda apenas com o JSON válido, sem texto adicional.";

            var response = await CallGeminiApiWithImageAsync(prompt, base64Image);
            return ParseGeminiResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar imagem do recibo com Gemini");
            return new GeminiReceiptAnalysis
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> ExtractReceiptDataAsync(string receiptText)
    {
        try
        {
            var prompt = $@"
Extraia e estruture os dados do seguinte recibo:

{receiptText}

Organize as informações de forma clara e estruturada, incluindo:
- Estabelecimento
- Data e hora
- Valor total
- Itens comprados
- Forma de pagamento (se disponível)
- Número de parcelas (se parcelado)
- Categoria sugerida

Formate a resposta de maneira legível e organizada.";

            var response = await CallGeminiApiAsync(prompt);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair dados do recibo com Gemini");
            return "Erro ao processar recibo";
        }
    }

    private async Task<string> CallGeminiApiAsync(string prompt)
    {
        var apiKey = _configuration["GoogleGemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Google Gemini API Key não configurada");
        }

        var requestBody = new
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
            },
            generationConfig = new
            {
                temperature = 0.1,
                topK = 1,
                topP = 1,
                maxOutputTokens = 2048
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
            content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro na API do Google Gemini: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Erro na API do Gemini: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent);

        return geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? string.Empty;
    }

    private async Task<string> CallGeminiApiWithImageAsync(string prompt, string base64Image)
    {
        var apiKey = _configuration["GoogleGemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Google Gemini API Key não configurada");
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new 
                        { 
                            inline_data = new 
                            { 
                                mime_type = "image/jpeg",
                                data = base64Image 
                            } 
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                topK = 1,
                topP = 1,
                maxOutputTokens = 2048
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
            content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro na API do Google Gemini: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Erro na API do Gemini: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent);

        return geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text ?? string.Empty;
    }

    private GeminiReceiptAnalysis ParseGeminiResponse(string response)
    {
        try
        {
            // Tentar extrair JSON da resposta
            var jsonMatch = Regex.Match(response, @"\{.*\}", RegexOptions.Singleline);
            if (!jsonMatch.Success)
            {
                return new GeminiReceiptAnalysis
                {
                    IsSuccessful = false,
                    ErrorMessage = "Resposta não contém JSON válido",
                    OriginalText = response
                };
            }

            var jsonString = jsonMatch.Value;
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            var analysis = new GeminiReceiptAnalysis
            {
                IsSuccessful = true,
                OriginalText = response
            };

            if (root.TryGetProperty("valor_total", out var valorTotal))
            {
                if (decimal.TryParse(valorTotal.GetString()?.Replace(",", "."), out var valor))
                {
                    analysis.ExtractedAmount = valor;
                }
            }

            if (root.TryGetProperty("estabelecimento", out var estabelecimento))
            {
                analysis.MerchantName = estabelecimento.GetString();
            }

            if (root.TryGetProperty("data", out var data))
            {
                if (DateTime.TryParse(data.GetString(), out var dataTransacao))
                {
                    analysis.TransactionDate = dataTransacao;
                }
            }

            if (root.TryGetProperty("categoria", out var categoria))
            {
                analysis.Category = categoria.GetString();
            }

            if (root.TryGetProperty("moeda", out var moeda))
            {
                analysis.Currency = moeda.GetString();
            }

            if (root.TryGetProperty("forma_pagamento", out var formaPagamento))
            {
                analysis.PaymentMethod = formaPagamento.GetString();
            }

            if (root.TryGetProperty("numero_parcelas", out var numeroParcelas))
            {
                if (numeroParcelas.ValueKind != JsonValueKind.Null)
                {
                    if (int.TryParse(numeroParcelas.GetString(), out var parcelas))
                    {
                        analysis.InstallmentCount = parcelas;
                    }
                    else if (numeroParcelas.ValueKind == JsonValueKind.Number)
                    {
                        analysis.InstallmentCount = numeroParcelas.GetInt32();
                    }
                }
            }

            if (root.TryGetProperty("itens", out var itens) && itens.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itens.EnumerateArray())
                {
                    var receiptItem = new ReceiptItem();
                    
                    if (item.TryGetProperty("nome", out var nome))
                    {
                        receiptItem.Name = nome.GetString() ?? string.Empty;
                    }
                    
                    if (item.TryGetProperty("valor", out var valorItem))
                    {
                        if (decimal.TryParse(valorItem.GetString()?.Replace(",", "."), out var preco))
                        {
                            receiptItem.Price = preco;
                        }
                    }
                    
                    if (item.TryGetProperty("quantidade", out var quantidade))
                    {
                        if (int.TryParse(quantidade.GetString(), out var qty))
                        {
                            receiptItem.Quantity = qty;
                        }
                    }

                    if (receiptItem.Price.HasValue && receiptItem.Quantity.HasValue)
                    {
                        receiptItem.Total = receiptItem.Price * receiptItem.Quantity;
                    }

                    analysis.Items.Add(receiptItem);
                }
            }

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer parse da resposta do Gemini");
            return new GeminiReceiptAnalysis
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                OriginalText = response
            };
        }
    }
}

// Classes para deserialização da resposta da API Gemini
internal class GeminiApiResponse
{
    public GeminiCandidate[]? candidates { get; set; }
}

internal class GeminiCandidate
{
    public GeminiContent? content { get; set; }
}

internal class GeminiContent
{
    public GeminiPart[]? parts { get; set; }
}

internal class GeminiPart
{
    public string? text { get; set; }
}
