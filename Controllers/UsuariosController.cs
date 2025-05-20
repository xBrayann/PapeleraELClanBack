using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly IConfiguration _configuration;

        public UsuariosController(FirebaseService firebaseService, IConfiguration configuration)
        {
            _firebaseService = firebaseService;
            _configuration = configuration;
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                new Claim("role", usuario.Rol ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };




            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var usuarios = await _firebaseService.GetAllUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var usuario = await _firebaseService.GetUsuarioByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest? request)



        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email es requerido");
            }

            var usuario = await _firebaseService.GetUsuarioByEmailAsync(request.Email!);



            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado");
            }

            if (string.IsNullOrEmpty(request.Contrasena) || string.IsNullOrEmpty(usuario.Contrasena))
            {
                return BadRequest("Contraseña inválida");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.Contrasena))


            {
                return Unauthorized("Credenciales inválidas");
            }

            var token = GenerateJwtToken(usuario);
            return Ok(new { Token = token, UserId = usuario.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.Nombre) || string.IsNullOrEmpty(usuario.Email))
            {
                return BadRequest("Usuario inválido.");
            }

            if (usuario.Email == "admin@example.com")
            {
                usuario.Rol = "admin";
            }
            else
            {
                usuario.Rol = "usuario";
            }

            if (!string.IsNullOrEmpty(usuario.Contrasena))
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
            }

            await _firebaseService.AddUsuarioAsync(usuario);
            return CreatedAtAction(nameof(Get), new { id = usuario.Id }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Usuario usuario)

        {
            var existingUsuario = await _firebaseService.GetUsuarioByIdAsync(id);
            if (existingUsuario == null)
            {
                return NotFound();
            }

            await _firebaseService.UpdateUsuarioAsync(id, usuario);
            return Ok(usuario);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var usuario = await _firebaseService.GetUsuarioByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            await _firebaseService.DeleteUsuarioAsync(id);
            return NoContent();
        }
    }
}
