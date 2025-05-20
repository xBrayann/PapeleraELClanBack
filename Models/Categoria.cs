using Google.Cloud.Firestore;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Categoria
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Descripcion { get; set; }
    }
}
