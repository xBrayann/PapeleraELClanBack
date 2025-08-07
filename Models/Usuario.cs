using BCrypt.Net;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Usuario
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        [FirestoreProperty]
        public string? Direccion { get; set; }

        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public string? Rol { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? Contrasena { get; set; }

        [FirestoreProperty]
        public string? FirebaseUid { get; set; }  // Firebase UID for email authentication

        [FirestoreProperty]
        public bool EmailVerificado { get; set; } = false;  // Email verification status
    }
}
