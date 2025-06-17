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
    public class MercadoPagoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MercadoPagoController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri("https://api.mercadopago.com/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["MercadoPago:AccessToken"]);
        }

        // POST: api/mercadopago/create_preference
        [AllowAnonymous]
        [HttpPost("create_preference")]
        public async Task<IActionResult> CreatePreference([FromBody] JsonElement preferenceRequest)
        {
            if (preferenceRequest.ValueKind == JsonValueKind.Undefined || preferenceRequest.ValueKind == JsonValueKind.Null)
            {
                return BadRequest("Datos de preferencia inválidos.");
            }

            try
            {
                var jsonContent = new StringContent(preferenceRequest.GetRawText(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("checkout/preferences", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error al crear la preferencia: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(JsonDocument.Parse(responseContent).RootElement);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error al crear la preferencia: {ex.Message}");
            }
        }

        // POST: api/mercadopago/webhook
        [AllowAnonymous]
        [HttpPost("webhook")]
        public IActionResult Webhook()
        {

            return Ok();
        }
    }
}
