using Microsoft.AspNetCore.Mvc;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Net;
using System.Net.Mail;

namespace PapeleriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IEmailService _emailService;

        public UsuariosController(FirebaseService firebaseService, IConfiguration configuration, ILogger<UsuariosController> logger, IEmailService emailService)
        {
            _firebaseService = firebaseService;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;

            // Initialize FirebaseApp if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var firebaseConfig = _configuration.GetSection("Firebase");
                var credentialsPath = firebaseConfig["CredentialsPath"];
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
            }
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
            try
            {
                var usuarios = await _firebaseService.GetAllUsuariosAsync();
                _logger.LogInformation("Usuarios obtenidos correctamente.");
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var usuario = await _firebaseService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", id);
                    return NotFound();
                }
                _logger.LogInformation("Usuario obtenido: {UserId}", id);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario: {UserId}", id);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        public class PhoneRequest
        {
            public string Telefono { get; set; } = string.Empty;
        }

        [HttpPost("sendVerificationCode")]
        public IActionResult SendVerificationCode([FromBody] PhoneRequest request)
        {
            try
            {
                var code = new Random().Next(100000, 999999).ToString();
                VerificationStorage.Codes[request.Telefono] = code;
                _logger.LogInformation("Código de verificación generado para: {Telefono} Código: {Code}", request.Telefono, code);
                return Ok(new { message = "Código enviado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código a: {Telefono}", request.Telefono);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("verifyCode")]
        public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                if (VerificationStorage.Codes.TryGetValue(request.Telefono, out var storedCode))
                {
                    if (storedCode == request.Codigo)
                    {
                        VerificationStorage.Codes.TryRemove(request.Telefono, out _);
                        _logger.LogInformation("Código verificado para: {Telefono}", request.Telefono);
                        return Ok("Código verificado");
                    }
                    else
                    {
                        _logger.LogWarning("Código inválido para: {Telefono}", request.Telefono);
                        return BadRequest("Código inválido");
                    }
                }
                else
                {
                    _logger.LogWarning("No se encontró código para: {Telefono}", request.Telefono);
                    return BadRequest("Código no encontrado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar código para: {Telefono}", request.Telefono);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest? request)
        {
            _logger.LogInformation("=== LOGIN ATTEMPT STARTED ===");
            
            if (request == null)
            {
                _logger.LogError("Login request body is null");
                return BadRequest(new { error = "Request body cannot be null", details = "Request body is null" });
            }

            _logger.LogInformation("Login request received - Email: {Email}, Password length: {PasswordLength}", 
                request.Email, request.Contrasena?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Login attempt with empty email");
                return BadRequest(new { error = "Email is required", details = "Email field is empty" });
            }

            if (string.IsNullOrWhiteSpace(request.Contrasena))
            {
                _logger.LogWarning("Login attempt with empty password");
                return BadRequest(new { error = "Password is required", details = "Password field is empty" });
            }

            // Validate email format
            if (!request.Email.Contains("@"))
            {
                _logger.LogWarning("Invalid email format: {Email}", request.Email);
                return BadRequest(new { error = "Invalid email format", details = "Email must contain @" });
            }

            try
            {
                _logger.LogInformation("Looking up user by email: {Email}", request.Email);
                var usuario = await _firebaseService.GetUsuarioByEmailAsync(request.Email!);

                if (usuario == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", request.Email);
                    return Unauthorized(new { error = "Usuario no encontrado", details = "No user exists with this email" });
                }

                _logger.LogInformation("User found: {UserId}, Email: {Email}, HasPassword: {HasPassword}", 
                    usuario.Id, usuario.Email, !string.IsNullOrEmpty(usuario.Contrasena));

                if (string.IsNullOrEmpty(usuario.Contrasena))
                {
                    _logger.LogError("User has no password hash stored: {Email}", request.Email);
                    return Unauthorized(new { error = "Account configuration error", details = "Password not properly configured" });
                }

                _logger.LogInformation("Attempting password verification...");
                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.Contrasena);
                _logger.LogInformation("Password verification result: {IsValid}", passwordValid);

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                    return Unauthorized(new { error = "Credenciales inválidas", details = "Password does not match" });
                }

                // Check if email is verified in Firebase
                _logger.LogInformation("Checking email verification status for user: {Email}", request.Email);
                try
                {
                    var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(request.Email);
                    if (!firebaseUser.EmailVerified)
                    {
                        _logger.LogWarning("User attempted login without verified email: {Email}", request.Email);
                        return Unauthorized(new { 
                            error = "Correo no verificado", 
                            details = "Por favor verifica tu correo antes de iniciar sesión." 
                        });
                    }
                }
                catch (FirebaseAuthException ex)
                {
                    _logger.LogError(ex, "Error checking email verification status for: {Email}", request.Email);
                    return Unauthorized(new { error = "Error al verificar estado del correo", details = ex.Message });
                }

                var token = GenerateJwtToken(usuario);
                _logger.LogInformation("Login successful for verified user: {Email}", request.Email);
                return Ok(new { 
                    Token = token, 
                    UserId = usuario.Id,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Rol = usuario.Rol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login for user: {Email}. Exception: {ExceptionMessage}", request.Email, ex.Message);
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.Nombre) || string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Contrasena))
            {
                _logger.LogWarning("Intento de crear usuario inválido.");
                return BadRequest("Usuario inválido. Nombre, email y contraseña son requeridos.");
            }

            try
            {
                // Verificar si el email ya existe (case-insensitive)
                var normalizedEmail = usuario.Email?.ToLower().Trim();
                var usuarioExistente = await _firebaseService.GetUsuarioByEmailAsync(normalizedEmail!);
                if (usuarioExistente != null)
                {
                    _logger.LogWarning("Email ya registrado: {Email}", normalizedEmail);
                    return Conflict(new { 
                        error = "EMAIL_EXISTS", 
                        message = "Este correo electrónico ya está registrado. Por favor, inicia sesión o utiliza un correo diferente.",
                        details = "El correo ya está asociado con una cuenta existente.",
                        suggestedAction = "login"
                    });
                }

                // Crear usuario en Firebase Authentication
                UserRecord userRecord;
                try
                {
                    var userArgs = new UserRecordArgs()
                    {
                        Email = usuario.Email,
                        Password = usuario.Contrasena,
                        DisplayName = usuario.Nombre,
                        EmailVerified = false
                    };
                    userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
                }
                catch (FirebaseAuthException ex)
                {
                    if (ex.Message.Contains("email already exists") || ex.Message.Contains("EMAIL_EXISTS"))
                    {
                        _logger.LogError(ex, "Error al crear usuario en Firebase - EMAIL_EXISTS: {Email}", usuario.Email);
                        return Conflict(new { 
                            error = "EMAIL_EXISTS", 
                            message = "This email address is already registered. Please login or use a different email.",
                            details = "The email is already associated with an existing account."
                        });
                    }
                    else
                    {
                        _logger.LogError(ex, "Error al crear usuario en Firebase: {Email}", usuario.Email);
                        return BadRequest(new { 
                            error = "FIREBASE_ERROR", 
                            message = "Error creating user in Firebase",
                            details = ex.Message 
                        });
                    }
                }

                    // Generar enlace de verificación de email
                    try
                    {
                        var email = usuario.Email;
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            var verificationLink = await FirebaseAuth.DefaultInstance.GenerateEmailVerificationLinkAsync(email);
                            _logger.LogInformation("Verification email generated for: {Email}", email);

                            // Enviar el correo con el link
                            string subject = "Verifica tu correo - Papelería Web";
                            string body = $@"
Hola {usuario.Nombre},

Gracias por registrarte en Papelería Web. Para verificar tu correo electrónico, por favor haz clic en el siguiente enlace:

{verificationLink}

Este enlace expirará en 24 horas.

Si no solicitaste este registro, puedes ignorar este correo.

Saludos,
Equipo de Papelería Web";

                            bool emailSent = await _emailService.SendEmailAsync(email, subject, body);

                            if (!emailSent)
                            {
                                _logger.LogWarning("No se pudo enviar el correo de verificación a: {Email}", email);
                            }
                            else
                            {
                                _logger.LogInformation("Correo de verificación enviado correctamente a: {Email}", email);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("El correo electrónico del usuario es nulo o vacío, no se pudo enviar verificación.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al generar o enviar enlace de verificación para: {Email}", usuario.Email);
                    }

                // Guardar usuario en Firestore
                usuario.FirebaseUid = userRecord.Uid;
                usuario.EmailVerificado = false;
                usuario.Rol = usuario.Email == "admin@example.com" ? "admin" : "usuario";
                
                // No almacenar la contraseña en Firestore (ya está en Firebase Auth)
                // usuario.Contrasena = null;
                // Instead, hash the password for Firestore storage (for login verification)
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

                // Ensure usuario.Id is set before saving
                if (string.IsNullOrEmpty(usuario.Id))
                {
                    usuario.Id = Guid.NewGuid().ToString();
                }

                await _firebaseService.AddUsuarioAsync(usuario);
                _logger.LogInformation("Usuario creado exitosamente: {Email} (Firebase UID: {Uid})", usuario.Email, userRecord.Uid);
                
                return CreatedAtAction(nameof(Get), new { id = usuario.Id }, new 
                { 
                    usuario.Id, 
                    usuario.Email, 
                    usuario.Nombre,
                    usuario.FirebaseUid,
                    usuario.EmailVerificado 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Email}", usuario.Email);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Usuario usuario)
        {
            try
            {
                var existingUsuario = await _firebaseService.GetUsuarioByIdAsync(id);
                if (existingUsuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para actualización: {UserId}", id);
                    return NotFound();
                }

                await _firebaseService.UpdateUsuarioAsync(id, usuario);
                _logger.LogInformation("Usuario actualizado: {UserId}", id);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {UserId}", id);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var usuario = await _firebaseService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para eliminación: {UserId}", id);
                    return NotFound();
                }

                await _firebaseService.DeleteUsuarioAsync(id);
                _logger.LogInformation("Usuario eliminado: {UserId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {UserId}", id);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("reenviar-verificacion")]
        public async Task<IActionResult> ReenviarCorreoVerificacion([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { error = "Email requerido", message = "El email es requerido para reenviar la verificación" });
                }

                var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                var verificationLink = await FirebaseAuth.DefaultInstance.GenerateEmailVerificationLinkAsync(email);

                // Enviar correo de verificación
                var emailSent = await _emailService.SendVerificationEmailAsync(email, verificationLink);
                
                if (emailSent)
                {
                    _logger.LogInformation("Correo de verificación enviado exitosamente a: {Email}", email);
                    return Ok(new { 
                        message = "Correo de verificación enviado exitosamente", 
                        email = email 
                    });
                }
                else
                {
                    _logger.LogError("Error al enviar correo de verificación a: {Email}", email);
                    return BadRequest(new { 
                        error = "Error al enviar correo", 
                        message = "No se pudo enviar el correo de verificación. Por favor, intenta nuevamente." 
                    });
                }
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError("Error al generar link de verificación: {Message}", ex.Message);
                return NotFound(new { error = "Usuario no encontrado" });
            }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO PRUEBA DE CORREO ===");
                _logger.LogInformation("Destinatario: {Email}", request.Email);
                
                var testLink = $"https://papeleriaweb.com/verify?token=test-{Guid.NewGuid()}";
                var success = await _emailService.SendVerificationEmailAsync(request.Email, testLink);
                
                if (success)
                {
                    _logger.LogInformation("✅ PRUEBA DE CORREO EXITOSA - Correo enviado a: {Email}", request.Email);
                    return Ok(new { 
                        message = "Correo de prueba enviado exitosamente", 
                        email = request.Email,
                        testLink = testLink 
                    });
                }
                else
                {
                    _logger.LogError("❌ PRUEBA DE CORREO FALLIDA - No se pudo enviar a: {Email}", request.Email);
                    return BadRequest(new { 
                        error = "Error al enviar correo de prueba", 
                        message = "Verifica la configuración SMTP y las credenciales" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR EN PRUEBA DE CORREO: {Message}", ex.Message);
                return StatusCode(500, new { 
                    error = "Error interno al enviar correo", 
                    details = ex.Message 
                });
            }
        }
    }

    public class VerifyCodeRequest
    {
        public string Telefono { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }

    public static class VerificationStorage
    {
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> Codes = new();
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
