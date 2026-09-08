using ControlAsistenciaWeb.Models;
using MySqlConnector;

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

        // ==========================================
        // MÉTODOS DE REPORTES (AVANCE #4)
        // ==========================================

        public async Task<List<ReporteAtrasoItem>> ObtenerAtrasosAsync(DateTime fechaInicio, DateTime fechaFin, TimeSpan horaEntradaEsperada, int toleranciaMinutos = 5)
        {
            var lista = new List<ReporteAtrasoItem>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    u.Id AS UsuarioId, 
                    u.NombreCompleto, 
                    u.Correo, 
                    r.FechaHora
                FROM RegistrosAsistencia r
                INNER JOIN Usuarios u ON r.UsuarioId = u.Id
                WHERE r.Tipo = 0
                  AND DATE(r.FechaHora) BETWEEN @fechaInicio AND @fechaFin
                  AND TIME(r.FechaHora) > @horaLimite
                ORDER BY r.FechaHora DESC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.Date);
            cmd.Parameters.AddWithValue("@fechaFin", fechaFin.Date);
            cmd.Parameters.AddWithValue("@horaLimite", horaEntradaEsperada.Add(TimeSpan.FromMinutes(toleranciaMinutos)));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DateTime fechaHora = reader.GetDateTime("FechaHora");
                int minutos = CalculoAsistenciaHelper.CalcularMinutosAtraso(fechaHora.TimeOfDay, horaEntradaEsperada, toleranciaMinutos);

                lista.Add(new ReporteAtrasoItem
                {
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    FechaHoraEntrada = fechaHora,
                    HoraPactada = horaEntradaEsperada,
                    MinutosAtraso = minutos
                });
            }

            return lista;
        }

        public async Task<List<ReporteSalidaAnticipadaItem>> ObtenerSalidasAnticipadasAsync(DateTime fechaInicio, DateTime fechaFin, TimeSpan horaSalidaEsperada)
        {
            var lista = new List<ReporteSalidaAnticipadaItem>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    u.Id AS UsuarioId, 
                    u.NombreCompleto, 
                    u.Correo, 
                    r.FechaHora
                FROM RegistrosAsistencia r
                INNER JOIN Usuarios u ON r.UsuarioId = u.Id
                WHERE r.Tipo = 1
                  AND DATE(r.FechaHora) BETWEEN @fechaInicio AND @fechaFin
                  AND TIME(r.FechaHora) < @horaSalidaEsperada
                ORDER BY r.FechaHora DESC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.Date);
            cmd.Parameters.AddWithValue("@fechaFin", fechaFin.Date);
            cmd.Parameters.AddWithValue("@horaSalidaEsperada", horaSalidaEsperada);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DateTime fechaHora = reader.GetDateTime("FechaHora");
                int minutos = CalculoAsistenciaHelper.CalcularMinutosSalidaAnticipada(fechaHora.TimeOfDay, horaSalidaEsperada);

                lista.Add(new ReporteSalidaAnticipadaItem
                {
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    FechaHoraSalida = fechaHora,
                    HoraPactada = horaSalidaEsperada,
                    MinutosAnticipacion = minutos
                });
            }

            return lista;
        }

        public async Task<List<ReporteInasistenciaItem>> ObtenerInasistenciasAsync(DateTime fecha)
        {
            var lista = new List<ReporteInasistenciaItem>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    u.Id AS UsuarioId, 
                    u.NombreCompleto, 
                    u.Correo
                FROM Usuarios u
                WHERE u.Activo = TRUE
                  AND u.Id NOT IN (
                      SELECT DISTINCT r.UsuarioId 
                      FROM RegistrosAsistencia r 
                      WHERE DATE(r.FechaHora) = @fecha
                  )
                ORDER BY u.NombreCompleto ASC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@fecha", fecha.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ReporteInasistenciaItem
                {
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    Fecha = fecha.Date
                });
            }

            return lista;
        }
    }
}