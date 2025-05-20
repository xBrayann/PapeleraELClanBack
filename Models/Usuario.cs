using BCrypt.Net;
using Google.Cloud.Firestore;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Usuario
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Email { get; set; }

        [FirestoreProperty]
        public string? Direccion { get; set; }

        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public string? Rol { get; set; }

        [FirestoreProperty]
        public string? Contrasena { get; set; }  
    }
}
