using Google.Cloud.Firestore;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Opinion
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? UsuarioId { get; set; }
                
        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public int Calificacion { get; set; }

        [FirestoreProperty]
        public string? Comentario { get; set; }

        [FirestoreProperty]
        public DateTime Fecha { get; set; }
    }
}
