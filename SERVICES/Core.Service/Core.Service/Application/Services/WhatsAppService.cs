using System.Text.Json;
using System.Text;
using Core.Service.Application.Models;
using Core.Service.Application.Domain.Models;
using Core.Service.Infrastructure.Data.Repositories;

namespace Core.Service.Application.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IGoogleGeminiService _geminiService;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WhatsAppService> logger,
        IGoogleGeminiService geminiService,
        IReceiptRepository receiptRepository,
        IUsuarioRepository usuarioRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _geminiService = geminiService;
        _receiptRepository = receiptRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<string> DownloadMediaAsync(string mediaId)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("WhatsApp Access Token não configurado");
            }

            // Primeiro, obter a URL do media
            var mediaInfoUrl = $"https://graph.facebook.com/v18.0/{mediaId}";
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var mediaInfoResponse = await _httpClient.GetAsync(mediaInfoUrl);
            if (!mediaInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter informações do media: {StatusCode}", mediaInfoResponse.StatusCode);
                return string.Empty;
            }

            var mediaInfoJson = await mediaInfoResponse.Content.ReadAsStringAsync();
            var mediaInfo = JsonSerializer.Deserialize<JsonElement>(mediaInfoJson);
            
            if (!mediaInfo.TryGetProperty("url", out var urlElement))
            {
                _logger.LogError("URL do media não encontrada na resposta");
                return string.Empty;
            }

            var mediaUrl = urlElement.GetString();
            if (string.IsNullOrEmpty(mediaUrl))
            {
                _logger.LogError("URL do media está vazia");
                return string.Empty;
            }

            // Baixar o arquivo de media
            var mediaResponse = await _httpClient.GetAsync(mediaUrl);
            if (!mediaResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao baixar media: {StatusCode}", mediaResponse.StatusCode);
                return string.Empty;
            }

            var mediaBytes = await mediaResponse.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(mediaBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar media do WhatsApp");
            return string.Empty;
        }
    }

    public async Task<bool> SendTextMessageAsync(string phoneNumber, string message)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                throw new InvalidOperationException("WhatsApp Access Token ou Phone Number ID não configurados");
            }

            var requestBody = new WhatsAppSendMessageRequest
            {
                To = phoneNumber,
                Type = "text",
                Text = new WhatsAppSendTextMessage { Body = message }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync(
                $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages", 
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao enviar mensagem WhatsApp: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem WhatsApp");
            return false;
        }
    }

    public async Task<bool> ProcessReceiptImageAsync(string phoneNumber, string mediaId, string? caption = null)
    {
        try
        {
            // Baixar a imagem
            var base64Image = await DownloadMediaAsync(mediaId);
            if (string.IsNullOrEmpty(base64Image))
            {
                await SendTextMessageAsync(phoneNumber, "❌ Erro ao processar a imagem. Tente novamente.");
                return false;
            }

            // Processar com Google Gemini
            var geminiResult = await _geminiService.AnalyzeReceiptImageAsync(base64Image);
            
            if (!geminiResult.IsSuccessful)
            {
                await SendTextMessageAsync(phoneNumber, "❌ Não foi possível analisar o recibo. Verifique se a imagem está clara.");
                return false;
            }

            // Buscar ou criar usuário baseado no número do WhatsApp
            var usuario = await GetOrCreateUserByPhoneAsync(phoneNumber);

            // Criar diretório se não existir
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "receipts", "whatsapp");
            Directory.CreateDirectory(uploadsPath);

            // Salvar arquivo
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadsPath, fileName);
            var imageBytes = Convert.FromBase64String(base64Image);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Criar registro do recibo
            var receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                NomeArquivo = $"whatsapp_{DateTime.Now:yyyyMMdd_HHmmss}.jpg",
                CaminhoArquivo = filePath,
                TipoMime = "image/jpeg",
                Descricao = geminiResult.MerchantName ?? "Recibo WhatsApp",
                Valor = geminiResult.ExtractedAmount ?? 0,
                Categoria = geminiResult.Category ?? "Geral"
            };

            await _receiptRepository.CriarAsync(receipt);

            // Preparar resposta formatada
            var responseMessage = FormatReceiptAnalysis(geminiResult);
            await SendTextMessageAsync(phoneNumber, responseMessage);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar imagem de recibo do WhatsApp");
            await SendTextMessageAsync(phoneNumber, "❌ Erro interno ao processar o recibo. Tente novamente mais tarde.");
            return false;
        }
    }

    private async Task<Usuario> GetOrCreateUserByPhoneAsync(string phoneNumber)
    {
        // Tentar encontrar usuário pelo número do WhatsApp
        var usuario = await _usuarioRepository.ObterPorWhatsAppAsync(phoneNumber);
        
        if (usuario == null)
        {
            // Criar novo usuário
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = $"Usuário WhatsApp {phoneNumber}",
                Email = $"whatsapp_{phoneNumber.Replace("+", "").Replace(" ", "")}@zapfinance.com",
                WhatsAppNumber = phoneNumber,
                Documento = phoneNumber.Replace("+", "").Replace(" ", ""), // Usar número como documento temporário
                TipoDocumento = TipoDocumento.CPF, // Valor padrão
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };

            usuario = await _usuarioRepository.CriarAsync(usuario);
        }

        return usuario;
    }

    private string FormatReceiptAnalysis(GeminiReceiptAnalysis analysis)
    {
        var message = new StringBuilder();
        message.AppendLine("✅ *Recibo processado com sucesso!*");
        message.AppendLine();

        if (!string.IsNullOrEmpty(analysis.MerchantName))
        {
            message.AppendLine($"🏪 *Estabelecimento:* {analysis.MerchantName}");
        }

        if (analysis.ExtractedAmount.HasValue)
        {
            message.AppendLine($"💰 *Valor Total:* R$ {analysis.ExtractedAmount.Value:F2}");
        }

        if (analysis.TransactionDate.HasValue)
        {
            message.AppendLine($"📅 *Data:* {analysis.TransactionDate.Value:dd/MM/yyyy}");
        }

        if (!string.IsNullOrEmpty(analysis.Category))
        {
            message.AppendLine($"📂 *Categoria:* {analysis.Category}");
        }

        if (!string.IsNullOrEmpty(analysis.PaymentMethod))
        {
            message.AppendLine($"💳 *Forma de Pagamento:* {analysis.PaymentMethod}");
        }

        if (analysis.InstallmentCount.HasValue && analysis.InstallmentCount > 1)
        {
            message.AppendLine($"📊 *Parcelas:* {analysis.InstallmentCount}x");
        }

        if (analysis.Items.Any())
        {
            message.AppendLine();
            message.AppendLine("🛒 *Itens:*");
            foreach (var item in analysis.Items.Take(5)) // Limitar a 5 itens para não ficar muito longo
            {
                var itemText = $"• {item.Name}";
                if (item.Quantity.HasValue && item.Quantity > 1)
                {
                    itemText += $" (x{item.Quantity})";
                }
                if (item.Price.HasValue)
                {
                    itemText += $" - R$ {item.Price.Value:F2}";
                }
                message.AppendLine(itemText);
            }

            if (analysis.Items.Count > 5)
            {
                message.AppendLine($"... e mais {analysis.Items.Count - 5} itens");
            }
        }

        message.AppendLine();
        message.AppendLine("📱 Recibo salvo no seu ZapFinance!");

        return message.ToString();
    }
}
