using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public CategoriasController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categorias = await _firebaseService.GetAllCategoriasAsync();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var categoria = await _firebaseService.GetCategoriaByIdAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return Ok(categoria);
        }

       
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Categoria categoria)
        {
            if (categoria == null || string.IsNullOrEmpty(categoria.Nombre))
            {
                return BadRequest("Categoría inválida.");
            }

            await _firebaseService.AddCategoriaAsync(categoria);
            return CreatedAtAction(nameof(Get), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Categoria categoria)
        {
            var existingCategoria = await _firebaseService.GetCategoriaByIdAsync(id);
            if (existingCategoria == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateCategoriaAsync(id, categoria);
            return Ok(categoria);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var categoria = await _firebaseService.GetCategoriaByIdAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteCategoriaAsync(id);
            return NoContent();
        }
    }
}
