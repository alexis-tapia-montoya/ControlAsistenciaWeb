using MySqlConnector;
using ControlAsistenciaWeb.Models;

namespace ControlAsistenciaWeb.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<List<Usuario>> ObtenerUsuariosActivosAsync()
        {
            var lista = new List<Usuario>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT Id, NombreCompleto, Correo, RolUsuario FROM Usuarios WHERE Activo = TRUE", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32("Id"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    RolUsuario = (Rol)reader.GetByte("RolUsuario")
                });
            }
            return lista;
        }

        public async Task<List<Usuario>> ObtenerUsuariosConRostroAsync()
        {
            var lista = new List<Usuario>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT Id, NombreCompleto, Correo, DescriptorFacial FROM Usuarios WHERE Activo = TRUE", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32("Id"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    DescriptorFacial = reader.IsDBNull(reader.GetOrdinal("DescriptorFacial")) ? null : reader.GetString("DescriptorFacial")
                });
            }
            return lista;
        }

        public async Task<bool> GuardarDescriptorFacialAsync(int usuarioId, string descriptorJson)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Usuarios SET DescriptorFacial = @descriptor WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@descriptor", descriptorJson);
            cmd.Parameters.AddWithValue("@id", usuarioId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> RegistrarMarcaAsync(int usuarioId, TipoRegistro tipo)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO RegistrosAsistencia (UsuarioId, FechaHora, Tipo, MetodoVerificacion) VALUES (@uid, @fecha, @tipo, 'Facial')", conn);

            cmd.Parameters.AddWithValue("@uid", usuarioId);
            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@tipo", (int)tipo);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}