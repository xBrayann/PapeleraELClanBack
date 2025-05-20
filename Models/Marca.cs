using Google.Cloud.Firestore;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Marca
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public string? PaisOrigen { get; set; }
    }
}
