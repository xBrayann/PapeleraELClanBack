using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetodoPagoController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public MetodoPagoController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        // GET: api/metodopago
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var metodosPago = await _firebaseService.GetAllMetodosPagoAsync();
            return Ok(metodosPago);
        }

        // GET: api/metodopago/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var metodoPago = await _firebaseService.GetMetodoPagoByIdAsync(id);
            return metodoPago == null ? NotFound() : Ok(metodoPago);
        }

        // POST: api/metodopago
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MetodoPago metodoPago)
        {
            if (metodoPago == null || string.IsNullOrWhiteSpace(metodoPago.Nombre))
                return BadRequest("El nombre del método de pago es obligatorio.");

            await _firebaseService.AddMetodoPagoAsync(metodoPago);
            return CreatedAtAction(nameof(Get), new { id = metodoPago.Id }, metodoPago);
        }

        // PUT: api/metodopago/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] MetodoPago metodoPago)
        {
            if (metodoPago == null || string.IsNullOrWhiteSpace(metodoPago.Nombre))
                return BadRequest("Los datos del método de pago son inválidos.");

            var existe = await _firebaseService.GetMetodoPagoByIdAsync(id);
            if (existe == null)
                return NotFound();

            metodoPago.Id = id; // Asegura que el ID del body y la URL coincidan
            await _firebaseService.UpdateMetodoPagoAsync(id, metodoPago);
            return Ok(metodoPago);
        }

        // DELETE: api/metodopago/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existe = await _firebaseService.GetMetodoPagoByIdAsync(id);
            if (existe == null)
                return NotFound();

            await _firebaseService.DeleteMetodoPagoAsync(id);
            return NoContent();
        }
    }
}
