using Google.Cloud.Firestore;

namespace PapeleriaApi.Models


{
    [FirestoreData]
    public class Producto
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public double Precio { get; set; }

        [FirestoreProperty]
        public string? Categoria { get; set; }

        [FirestoreProperty]
        public string? Imagen { get; set; }

        [FirestoreProperty]
        public int Stock { get; set; }

        [FirestoreProperty]
        public string? Marca { get; set; }

        [FirestoreProperty]
        public string? Proveedor { get; set; }
    }
}
