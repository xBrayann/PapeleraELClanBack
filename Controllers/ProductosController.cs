using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public ProductosController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

       
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var productos = await _firebaseService.GetAllProductsAsync();
            return Ok(productos);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)  
        {
            var producto = await _firebaseService.GetProductByIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

       
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Producto producto)
        {
            if (producto == null || string.IsNullOrEmpty(producto?.Nombre) || producto.Precio <= 0)
            {
                return BadRequest("Producto inválido.");
            }

            await _firebaseService.AddProductAsync(producto);
            return CreatedAtAction(nameof(Get), new { id = producto.Id }, producto);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Producto producto)  // Cambié el tipo de 'id' a string
        {
            var existingProducto = await _firebaseService.GetProductByIdAsync(id);  // Método optimizado

            if (existingProducto == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateProductAsync(id, producto);
            return Ok(producto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)  // Cambié el tipo de 'id' a string
        {
            var producto = await _firebaseService.GetProductByIdAsync(id);  // Método optimizado

            if (producto == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
