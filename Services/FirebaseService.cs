using PapeleriaApi.Models;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PapeleriaApi.Services
{
    public class FirebaseService
    {
        private static FirestoreDb? _firestoreDb;

        private readonly IConfiguration _configuration;

        public FirebaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            if (_firestoreDb == null)
            {
                var firebaseConfig = _configuration.GetSection("Firebase");
                var credentialsPath = firebaseConfig["CredentialsPath"];
                var projectId = firebaseConfig["ProjectId"];

                if (string.IsNullOrEmpty(credentialsPath) || string.IsNullOrEmpty(projectId))
                {
                    throw new InvalidOperationException("La configuración de Firebase no está completa en appsettings.json.");
                }

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });

                _firestoreDb = FirestoreDb.Create(projectId);
            }
        }


        public async Task<List<Producto>> GetAllProductsAsync()
        {
            var productos = new List<Producto>();
            var snapshot = await _firestoreDb!.Collection("productos").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var producto = document.ConvertTo<Producto>();
                productos.Add(producto);
            }
            return productos;
        }

        // Métodos para la entidad MetodoPago
        public async Task<List<MetodoPago>> GetAllMetodosPagoAsync()
        {
            var metodosPago = new List<MetodoPago>();
            var snapshot = await _firestoreDb!.Collection("metodosPago").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var metodoPago = document.ConvertTo<MetodoPago>();
                metodosPago.Add(metodoPago);
            }
            return metodosPago;
        }

        public async Task<MetodoPago?> GetMetodoPagoByIdAsync(string id)
        {
            var document = await _firestoreDb!.Collection("metodosPago").Document(id).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<MetodoPago>() : null;
        }

        public async Task AddMetodoPagoAsync(MetodoPago metodoPago)
        {
            var docRef = _firestoreDb!.Collection("metodosPago").Document();
            metodoPago.Id = docRef.Id;
            await docRef.SetAsync(metodoPago);
        }

        public async Task<bool> UpdateMetodoPagoAsync(string id, MetodoPago metodoPago)
        {
            var docRef = _firestoreDb!.Collection("metodosPago").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(metodoPago, SetOptions.MergeAll);
            return true;
        }

        public async Task<bool> DeleteMetodoPagoAsync(string id)
        {
            var docRef = _firestoreDb!.Collection("metodosPago").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }

        public async Task<Producto?> GetProductByIdAsync(string id)
        {
            var document = await _firestoreDb!.Collection("productos").Document(id).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<Producto>() : null;
        }

        public async Task<List<Producto>> SearchProductsAsync(string? nombre, string? categoria)
        {
            var productos = new List<Producto>();
            try
            {
                CollectionReference productosRef = _firestoreDb!.Collection("productos");
                Query query = productosRef;

                if (!string.IsNullOrEmpty(nombre))
                {
                    query = query.WhereGreaterThanOrEqualTo("Nombre", nombre)
                                 .WhereLessThanOrEqualTo("Nombre", nombre + "\uf8ff");
                }
                if (!string.IsNullOrEmpty(categoria) && categoria != "Todas")
                {
                    query = query.WhereEqualTo("Categoria", categoria);
                }

                var snapshot = await query.GetSnapshotAsync();
                foreach (var document in snapshot.Documents)
                {
                    productos.Add(document.ConvertTo<Producto>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar productos: {ex.Message}");
            }
            return productos;
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            var categorias = new HashSet<string>();
            try
            {
                var snapshot = await _firestoreDb!.Collection("productos").GetSnapshotAsync();
                foreach (var document in snapshot.Documents)
                {
                    if (document.ContainsField("Categoria"))
                    {
                        categorias.Add(document.GetValue<string>("Categoria"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categorías: {ex.Message}");
            }
            return new List<string>(categorias);
        }

        public async Task AddProductAsync(Producto producto)
        {
            var docRef = _firestoreDb!.Collection("productos").Document(); 
            producto.Id = docRef.Id; 
            await docRef.SetAsync(producto);
        }

    
        public async Task<bool> UpdateProductAsync(string id, Producto producto)
        {
            var docRef = _firestoreDb!.Collection("productos").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(producto, SetOptions.MergeAll);
            return true;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var docRef = _firestoreDb!.Collection("productos").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }
        public async Task<List<Usuario>> GetAllUsuariosAsync()
        {
            var usuarios = new List<Usuario>();
            var snapshot = await _firestoreDb!.Collection("usuarios").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var usuario = document.ConvertTo<Usuario>();
                usuarios.Add(usuario);
            }
            return usuarios;
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(string id)
        {
            var document = await _firestoreDb!.Collection("usuarios").Document(id).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<Usuario>() : null;
        }

        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            var query = _firestoreDb!.Collection("usuarios")
                .WhereEqualTo("Email", email)
                .Limit(1);
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault()?.ConvertTo<Usuario>();
        }

        public async Task AddUsuarioAsync(Usuario usuario)
        {
            var docRef = _firestoreDb!.Collection("usuarios").Document(); // Genera un ID único
            usuario.Id = docRef.Id; // Asigna el ID generado por Firestore
            await docRef.SetAsync(usuario);
        }
        public async Task<bool> UpdateUsuarioAsync(string id, Usuario usuario)
        {
            var docRef = _firestoreDb!.Collection("usuarios").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(usuario, SetOptions.MergeAll);
            return true;
        }

        public async Task<bool> DeleteUsuarioAsync(string id)
        {
            var docRef = _firestoreDb!.Collection("usuarios").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }

        public async Task<List<Orden>> GetAllOrdenesAsync()
        {
            var ordenes = new List<Orden>();
            var snapshot = await _firestoreDb!.Collection("ordenes").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var orden = document.ConvertTo<Orden>();
                ordenes.Add(orden);
            }
            return ordenes;
        }

        public async Task<Orden?> GetOrdenByIdAsync(string id)
        {
            var document = await _firestoreDb!.Collection("ordenes").Document(id).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<Orden>() : null;
        }

        public async Task AddOrdenAsync(Orden orden)
        {
            var docRef = _firestoreDb!.Collection("ordenes").Document(); // Genera un ID único
            orden.Id = docRef.Id; // Asigna el ID generado por Firestore
            await docRef.SetAsync(orden);
        }

        public async Task<bool> UpdateOrdenAsync(string id, Orden orden)
        {
            var docRef = _firestoreDb!.Collection("ordenes").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(orden, SetOptions.MergeAll);
            return true;
        }

        public async Task<bool> DeleteOrdenAsync(string id)
        {
            var docRef = _firestoreDb!.Collection("ordenes").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }

        public async Task<List<Categoria>> GetAllCategoriasAsync()
        {
            var categorias = new List<Categoria>();
            var snapshot = await _firestoreDb!.Collection("categorias").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var categoria = document.ConvertTo<Categoria>();
                categorias.Add(categoria);
            }
            return categorias;
        }
public async Task<Categoria?> GetCategoriaByIdAsync(string id)
{
    var document = await _firestoreDb!.Collection("categorias").Document(id).GetSnapshotAsync();
    return document.Exists ? document.ConvertTo<Categoria>() : null;
}


        public async Task AddCategoriaAsync(Categoria categoria)
        {
            var docRef = _firestoreDb!.Collection("categorias").Document(); // Genera un ID único
            categoria.Id = docRef.Id; // Asigna el ID generado por Firestore
            await docRef.SetAsync(categoria);
        }
public async Task<bool> UpdateCategoriaAsync(string id, Categoria categoria)
{
    var docRef = _firestoreDb!.Collection("categorias").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();
    
    if (!snapshot.Exists)
    {
        return false;
    }

    await docRef.SetAsync(categoria, SetOptions.MergeAll);
    return true;
}
public async Task<bool> DeleteCategoriaAsync(string id)
{
    var docRef = _firestoreDb!.Collection("categorias").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();

    if (!snapshot.Exists)
    {
        return false;
    }

    await docRef.DeleteAsync();
    return true;
}


        public async Task<List<Proveedor>> GetAllProveedoresAsync()
        {
            var proveedores = new List<Proveedor>();
            var snapshot = await _firestoreDb!.Collection("proveedores").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var proveedor = document.ConvertTo<Proveedor>();
                proveedores.Add(proveedor);
            }
            return proveedores;
        }
public async Task<Proveedor?> GetProveedorByIdAsync(string id)
{
    var document = await _firestoreDb!.Collection("proveedores").Document(id).GetSnapshotAsync();
    return document.Exists ? document.ConvertTo<Proveedor>() : null;
}
public async Task<bool> UpdateProveedorAsync(string id, Proveedor proveedor)
{
    var docRef = _firestoreDb!.Collection("proveedores").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();

    if (!snapshot.Exists)
    {
        return false;
    }

    await docRef.SetAsync(proveedor, SetOptions.MergeAll); // MergeAll preserva los datos existentes que no se actualicen.
    return true;
}
public async Task<bool> DeleteProveedorAsync(string id)
{
    var docRef = _firestoreDb!.Collection("proveedores").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();

    if (!snapshot.Exists)
    {
        return false; 
    }

    await docRef.DeleteAsync(); 
    return true;
}


        public async Task AddProveedorAsync(Proveedor proveedor)
        {
            var docRef = _firestoreDb!.Collection("proveedores").Document(); 
            proveedor.Id = docRef.Id; 
            await docRef.SetAsync(proveedor);
        }

        public async Task<List<Marca>> GetAllMarcasAsync()
        {
            var marcas = new List<Marca>();
            var snapshot = await _firestoreDb!.Collection("marcas").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var marca = document.ConvertTo<Marca>();
                marcas.Add(marca);
            }
            return marcas;
        }
public async Task<Marca?> GetMarcaByIdAsync(string id)
{
    var document = await _firestoreDb!.Collection("marcas").Document(id).GetSnapshotAsync();
    return document.Exists ? document.ConvertTo<Marca>() : null;
}


        public async Task AddMarcaAsync(Marca marca)
        {
            var docRef = _firestoreDb!.Collection("marcas").Document(); 
            marca.Id = docRef.Id; 
            await docRef.SetAsync(marca);
        }
public async Task<bool> UpdateMarcaAsync(string id, Marca marca)
{
    var docRef = _firestoreDb!.Collection("marcas").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();
    
    if (!snapshot.Exists)
    {
        return false; 
    }

    await docRef.SetAsync(marca, SetOptions.MergeAll);
    return true;
}

public async Task<bool> DeleteMarcaAsync(string id)
{
    var docRef = _firestoreDb!.Collection("marcas").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();

    if (!snapshot.Exists)
    {
        return false; 
    }

    await docRef.DeleteAsync(); 
    return true;
}
        public async Task<List<Opinion>> GetAllOpinionesAsync()
        {
            var opiniones = new List<Opinion>();
            var snapshot = await _firestoreDb!.Collection("opiniones").GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                var opinion = document.ConvertTo<Opinion>();
                opiniones.Add(opinion);
            }
            return opiniones;
        }

        public async Task<Opinion?> GetOpinionByIdAsync(string id)
        {
            var document = await _firestoreDb!.Collection("opiniones").Document(id).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<Opinion>() : null;
        }

        public async Task AddOpinionAsync(Opinion opinion)
        {
            var docRef = _firestoreDb!.Collection("opiniones").Document(); 
            opinion.Id = docRef.Id; 
            await docRef.SetAsync(opinion);
        }

        public async Task<bool> UpdateOpinionAsync(string id, Opinion opinion)
        {
            var docRef = _firestoreDb!.Collection("opiniones").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.SetAsync(opinion, SetOptions.MergeAll);
            return true;
        }

        public async Task<bool> DeleteOpinionAsync(string id)
        {
            var docRef = _firestoreDb!.Collection("opiniones").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }

        
    }
}
