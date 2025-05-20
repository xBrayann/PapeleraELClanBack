using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public OrdenesController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ordenes = await _firebaseService.GetAllOrdenesAsync();
            return Ok(ordenes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var orden = await _firebaseService.GetOrdenByIdAsync(id);
            if (orden == null)
            {
                return NotFound();
            }
            return Ok(orden);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Orden orden)
        {
            if (orden == null ||  string.IsNullOrEmpty(orden.UsuarioId))
            {
                return BadRequest("Orden inválida.");
            }

            await _firebaseService.AddOrdenAsync(orden);
            return CreatedAtAction(nameof(Get), new { id = 1 }, orden);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Orden orden)
        {
            var existingOrden = await _firebaseService.GetOrdenByIdAsync(id);
            if (existingOrden == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateOrdenAsync(id, orden);
            return Ok(orden);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var orden = await _firebaseService.GetOrdenByIdAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteOrdenAsync(id);
            return NoContent();
        }
    }
}
