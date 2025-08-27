namespace Core.Service.Application.Services;

public interface IGoogleGeminiService
{
    Task<string> AnalyzeReceiptTextAsync(string receiptText);
    Task<GeminiReceiptAnalysis> AnalyzeReceiptImageAsync(string base64Image);
    Task<string> ExtractReceiptDataAsync(string receiptText);
}

public class GeminiReceiptAnalysis
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? ExtractedAmount { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? Category { get; set; }
    public List<ReceiptItem> Items { get; set; } = new();
    public string? Currency { get; set; }
    public string? PaymentMethod { get; set; }
    public int? InstallmentCount { get; set; }
    public string? OriginalText { get; set; }
}

public class ReceiptItem
{
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public decimal? Total { get; set; }
}
