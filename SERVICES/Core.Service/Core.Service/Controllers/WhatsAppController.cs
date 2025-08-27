using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Core.Service.Application.Models;
using Core.Service.Application.Services;

namespace Core.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IWhatsAppService whatsAppService,
        IConfiguration configuration,
        ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                      [FromQuery(Name = "hub.challenge")] string challenge,
                                      [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        try
        {
            var expectedToken = _configuration["WhatsApp:VerifyToken"];
            
            if (mode == "subscribe" && verifyToken == expectedToken)
            {
                _logger.LogInformation("Webhook verificado com sucesso");
                return Ok(challenge);
            }
            
            _logger.LogWarning("Falha na verificação do webhook. Mode: {Mode}, Token válido: {TokenValid}", 
                mode, verifyToken == expectedToken);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na verificação do webhook");
            return StatusCode(500);
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveWebhook([FromBody] WhatsAppWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Webhook recebido: {Request}", JsonSerializer.Serialize(request));

            if (request.Object != "whatsapp_business_account")
            {
                return BadRequest("Objeto não suportado");
            }

            foreach (var entry in request.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    if (change.Field == "messages")
                    {
                        await ProcessMessagesAsync(change.Value);
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook");
            return StatusCode(500);
        }
    }

    private async Task ProcessMessagesAsync(WhatsAppValue value)
    {
        try
        {
            foreach (var message in value.Messages)
            {
                var phoneNumber = message.From;
                var messageType = message.Type;

                _logger.LogInformation("Processando mensagem do tipo {Type} de {Phone}", messageType, phoneNumber);

                switch (messageType)
                {
                    case "text":
                        await ProcessTextMessageAsync(phoneNumber, message.Text?.Body ?? string.Empty);
                        break;
                    
                    case "image":
                        if (message.Image != null)
                        {
                            await ProcessImageMessageAsync(phoneNumber, message.Image);
                        }
                        break;
                    
                    default:
                        await _whatsAppService.SendTextMessageAsync(phoneNumber, 
                            "📱 Olá! Envie uma foto do seu recibo que eu vou analisar para você! 📸");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagens");
        }
    }

    private async Task ProcessTextMessageAsync(string phoneNumber, string messageText)
    {
        try
        {
            var lowerMessage = messageText.ToLowerInvariant().Trim();

            // Comandos básicos
            switch (lowerMessage)
            {
                case "oi":
                case "olá":
                case "ola":
                case "hello":
                case "hi":
                    await _whatsAppService.SendTextMessageAsync(phoneNumber, 
                        "👋 Olá! Bem-vindo ao *ZapFinance*!\n\n" +
                        "📸 Envie uma foto do seu recibo que eu vou extrair todas as informações automaticamente!\n\n" +
                        "✨ Posso identificar:\n" +
                        "• Valor total\n" +
                        "• Estabelecimento\n" +
                        "• Data da compra\n" +
                        "• Itens comprados\n" +
                        "• Número de parcelas\n" +
                        "• Categoria\n\n" +
                        "Vamos começar? 🚀");
                    break;

                case "ajuda":
                case "help":
                case "?":
                    await _whatsAppService.SendTextMessageAsync(phoneNumber,
                        "🆘 *Como usar o ZapFinance:*\n\n" +
                        "1️⃣ Tire uma foto clara do seu recibo\n" +
                        "2️⃣ Envie a foto aqui no WhatsApp\n" +
                        "3️⃣ Aguarde a análise automática\n" +
                        "4️⃣ Receba todas as informações extraídas!\n\n" +
                        "💡 *Dicas para melhores resultados:*\n" +
                        "• Foto bem iluminada\n" +
                        "• Recibo completamente visível\n" +
                        "• Evite sombras ou reflexos\n\n" +
                        "📱 Seus recibos ficam salvos automaticamente!");
                    break;

                default:
                    await _whatsAppService.SendTextMessageAsync(phoneNumber,
                        "📸 Envie uma foto do seu recibo para análise!\n\n" +
                        "💬 Digite *ajuda* se precisar de instruções.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem de texto");
        }
    }

    private async Task ProcessImageMessageAsync(string phoneNumber, WhatsAppImageMessage image)
    {
        try
        {
            await _whatsAppService.SendTextMessageAsync(phoneNumber, 
                "🔍 Analisando seu recibo... Aguarde um momento!");

            var success = await _whatsAppService.ProcessReceiptImageAsync(
                phoneNumber, 
                image.Id, 
                image.Caption);

            if (!success)
            {
                await _whatsAppService.SendTextMessageAsync(phoneNumber,
                    "❌ Não consegui processar esta imagem.\n\n" +
                    "💡 *Tente novamente com:*\n" +
                    "• Foto mais clara\n" +
                    "• Melhor iluminação\n" +
                    "• Recibo completamente visível\n\n" +
                    "📸 Envie outra foto quando estiver pronto!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar imagem");
            await _whatsAppService.SendTextMessageAsync(phoneNumber,
                "❌ Erro interno ao processar a imagem. Tente novamente em alguns instantes.");
        }
    }
}
