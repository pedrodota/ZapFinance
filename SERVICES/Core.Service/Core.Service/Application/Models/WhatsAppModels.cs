using System.Text.Json.Serialization;

namespace Core.Service.Application.Models;

// Modelos para receber webhooks do WhatsApp
public class WhatsAppWebhookRequest
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<WhatsAppEntry> Entry { get; set; } = new();
}

public class WhatsAppEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<WhatsAppChange> Changes { get; set; } = new();
}

public class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppValue Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class WhatsAppValue
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public WhatsAppMetadata Metadata { get; set; } = new();

    [JsonPropertyName("contacts")]
    public List<WhatsAppContact> Contacts { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<WhatsAppMessage> Messages { get; set; } = new();
}

public class WhatsAppMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class WhatsAppContact
{
    [JsonPropertyName("profile")]
    public WhatsAppProfile Profile { get; set; } = new();

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class WhatsAppProfile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public WhatsAppTextMessage? Text { get; set; }

    [JsonPropertyName("image")]
    public WhatsAppImageMessage? Image { get; set; }
}

public class WhatsAppTextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class WhatsAppImageMessage
{
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

// Modelos para enviar mensagens
public class WhatsAppSendMessageRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = "whatsapp";

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public WhatsAppSendTextMessage? Text { get; set; }
}

public class WhatsAppSendTextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class WhatsAppSendMessageResponse
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("contacts")]
    public List<WhatsAppSendContact> Contacts { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<WhatsAppSendMessageResult> Messages { get; set; } = new();
}

public class WhatsAppSendContact
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class WhatsAppSendMessageResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
