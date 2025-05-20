using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcaController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public MarcaController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var marcas = await _firebaseService.GetAllMarcasAsync();
            return Ok(marcas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var marca = await _firebaseService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
                return NotFound();
            }
            return Ok(marca);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Marca marca)
        {
            if (marca == null || string.IsNullOrEmpty(marca.Nombre))
            {
                return BadRequest("Marca inválida.");
            }

            await _firebaseService.AddMarcaAsync(marca);
            return CreatedAtAction(nameof(Get), new { id = marca.Id }, marca);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Marca marca)
        {
            var existingMarca = await _firebaseService.GetMarcaByIdAsync(id);
            if (existingMarca == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateMarcaAsync(id, marca);
            return Ok(marca);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var marca = await _firebaseService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteMarcaAsync(id);
            return NoContent();
        }
    }
}
