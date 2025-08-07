using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PayPalController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri("https://api-m.sandbox.paypal.com/"); // Use sandbox for testing
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var secret = _configuration["PayPal:Secret"];

            var authToken = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrEmpty(token))
            {
                throw new System.Exception("Failed to retrieve PayPal access token.");
            }
            return token;
        }

        [AllowAnonymous]
        [HttpPost("create_order")]
        public async Task<IActionResult> CreateOrder([FromBody] JsonElement orderRequest)
        {
            if (orderRequest.ValueKind == JsonValueKind.Undefined || orderRequest.ValueKind == JsonValueKind.Null)
            {
                return BadRequest("Invalid order data.");
            }

            try
            {
                var accessToken = await GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonContent = new StringContent(orderRequest.GetRawText(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("v2/checkout/orders", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"PayPal create order error: {errorContent}");
                    System.Console.WriteLine($"Request payload: {orderRequest.GetRawText()}");
                    return StatusCode((int)response.StatusCode, $"Error creating order: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(JsonDocument.Parse(responseContent).RootElement);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error creating order: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public IActionResult Webhook()
        {
            return Ok();
        }
    }
}
