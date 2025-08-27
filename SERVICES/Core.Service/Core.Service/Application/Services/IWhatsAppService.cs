using Core.Service.Application.Models;

namespace Core.Service.Application.Services;

public interface IWhatsAppService
{
    Task<string> DownloadMediaAsync(string mediaId);
    Task<bool> SendTextMessageAsync(string phoneNumber, string message);
    Task<bool> ProcessReceiptImageAsync(string phoneNumber, string mediaId, string? caption = null);
}
