using Google.Cloud.Firestore;
using System;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class MetodoPago
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Nombre { get; set; }

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public bool Activo { get; set; }
    }
}
