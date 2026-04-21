namespace SmartCenter.Service.SePayService;

public class SePayOptions
{
     public required string ApiKey { get; set; } = string.Empty;
     public required string WebhookToken { get; set; } = string.Empty;
}