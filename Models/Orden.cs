using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace PapeleriaApi.Models
{
    [FirestoreData]
    public class Orden
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? UsuarioId { get; set; }

        [FirestoreProperty]
        public DateTime Fecha { get; set; }

        [FirestoreProperty]
        public List<OrdenItem>? Items { get; set; }

        [FirestoreProperty]
        public double Total { get; set; }

        [FirestoreProperty]
        public string? Estado { get; set; }
    }

    [FirestoreData]
    public class OrdenItem
    {
        [FirestoreProperty]
        public string? ProductoId { get; set; }

        [FirestoreProperty]
        public int Cantidad { get; set; }

        [FirestoreProperty]
        public double PrecioUnitario { get; set; }
    }
}
