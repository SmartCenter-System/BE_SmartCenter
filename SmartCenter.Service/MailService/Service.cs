using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace SmartCenter.Service.MailService;

public class Service : IService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _displayName;
    private readonly HttpClient _httpClient;

    public Service(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _apiKey      = config["ResendOptions:ApiKey"]!;
        _fromEmail   = config["ResendOptions:FromEmail"]!;
        _displayName = config["ResendOptions:DisplayName"]!;
        _httpClient  = httpClientFactory.CreateClient();
    }

    public async Task SendMail(MailContent mailContent)
    {
        var payload = new
        {
            from    = $"{_displayName} <{_fromEmail}>",
            to      = new[] { mailContent.To },
            subject = mailContent.Subject,
            html    = mailContent.Body,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Resend API error: {error}");
        }
    }
}