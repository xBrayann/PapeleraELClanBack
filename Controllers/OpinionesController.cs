using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpinionesController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public OpinionesController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var opiniones = await _firebaseService.GetAllOpinionesAsync();
            return Ok(opiniones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var opinion = await _firebaseService.GetOpinionByIdAsync(id);
            if (opinion == null)
            {
                return NotFound();
            }
            return Ok(opinion);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PapeleriaApi.Models.Opinion opinion)

        {
            if (opinion == null || string.IsNullOrEmpty(opinion.UsuarioId))
            {
                return BadRequest("Opinión inválida.");
            }

            await _firebaseService.AddOpinionAsync(opinion);
            return CreatedAtAction(nameof(Get), new { id = opinion.Id }, opinion);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] PapeleriaApi.Models.Opinion opinion)

        {
            var existingOpinion = await _firebaseService.GetOpinionByIdAsync(id);
            if (existingOpinion == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateOpinionAsync(id, opinion);
            return Ok(opinion);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var opinion = await _firebaseService.GetOpinionByIdAsync(id);
            if (opinion == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteOpinionAsync(id);
            return NoContent();
        }
    }
}
