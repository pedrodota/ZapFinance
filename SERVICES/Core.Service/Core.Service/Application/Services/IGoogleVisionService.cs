namespace Core.Service.Application.Services;

public interface IGoogleVisionService
{
    Task<string> ExtractTextFromImageAsync(string base64Image);
    Task<GoogleVisionResult> AnalyzeReceiptAsync(string base64Image);
}

public class GoogleVisionResult
{
    public string? ExtractedText { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}
