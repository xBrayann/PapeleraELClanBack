using Google.Cloud.Firestore;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Proveedor
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Direccion { get; set; }

        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public string? Email { get; set; }
    }
}
