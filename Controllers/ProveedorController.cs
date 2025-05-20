using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public ProveedorController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var proveedores = await _firebaseService.GetAllProveedoresAsync();
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var proveedor = await _firebaseService.GetProveedorByIdAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Proveedor proveedor)
        {
            if (proveedor == null || string.IsNullOrEmpty(proveedor?.Nombre))
            {
                return BadRequest("Proveedor inválido.");
            }

            await _firebaseService.AddProveedorAsync(proveedor);
            return CreatedAtAction(nameof(Get), new { id = proveedor.Id }, proveedor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Proveedor proveedor)
        {
            var existingProveedor = await _firebaseService.GetProveedorByIdAsync(id);
            if (existingProveedor == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateProveedorAsync(id, proveedor);
            return Ok(proveedor);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var proveedor = await _firebaseService.GetProveedorByIdAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteProveedorAsync(id);
            return NoContent();
        }
    }
}
