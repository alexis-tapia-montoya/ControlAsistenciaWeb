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

        public async Task<List<HistorialTrabajadorItem>> ObtenerHistorialPersonalAsync(int usuarioId)
        {
            var lista = new List<HistorialTrabajadorItem>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT FechaHora, Tipo FROM RegistrosAsistencia 
                             WHERE UsuarioId = @usuarioId 
                             ORDER BY FechaHora DESC LIMIT 30";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var fechaHora = reader.GetDateTime("FechaHora");
                var tipo = (TipoRegistro)reader.GetInt32("Tipo");

                string estado = "A tiempo";
                if (tipo == TipoRegistro.entrada && fechaHora.TimeOfDay > new TimeSpan(9, 30, 0))
                {
                    int minutos = (int)(fechaHora.TimeOfDay - new TimeSpan(9, 30, 0)).TotalMinutes;
                    estado = $"Atraso ({minutos} min)";
                }
                else if (tipo == TipoRegistro.salida && fechaHora.TimeOfDay < new TimeSpan(17, 30, 0))
                {
                    int minutos = (int)(new TimeSpan(17, 30, 0) - fechaHora.TimeOfDay).TotalMinutes;
                    estado = $"Salida Temprana ({minutos} min)";
                }

                lista.Add(new HistorialTrabajadorItem
                {
                    FechaHora = fechaHora,
                    Tipo = tipo,
                    Estado = estado
                });
            }
            return lista;
        }

        public async Task<bool> GuardarJustificacionAsync(int usuarioId, DateTime fechaAusencia, string motivo, string nombreArchivo, string rutaArchivo)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string initTableQuery = @"
                CREATE TABLE IF NOT EXISTS JUSTIFICACIONES (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UsuarioId INT NOT NULL,
                    FechaAusencia DATE NOT NULL,
                    Motivo VARCHAR(255) NOT NULL,
                    NombreArchivo VARCHAR(255) NOT NULL,
                    RutaArchivo VARCHAR(500) NOT NULL,
                    FechaSubida DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                );";

            using (var initCmd = new MySqlCommand(initTableQuery, conn))
            {
                await initCmd.ExecuteNonQueryAsync();
            }

            string query = @"INSERT INTO JUSTIFICACIONES (UsuarioId, FechaAusencia, Motivo, NombreArchivo, RutaArchivo) 
                             VALUES (@usuarioId, @fecha, @motivo, @nombre, @ruta)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
            cmd.Parameters.AddWithValue("@fecha", fechaAusencia.Date);
            cmd.Parameters.AddWithValue("@motivo", motivo);
            cmd.Parameters.AddWithValue("@nombre", nombreArchivo);
            cmd.Parameters.AddWithValue("@ruta", rutaArchivo);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<int> CrearUsuarioAsync(string nombreCompleto, string correo, Rol rol)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO Usuarios (NombreCompleto, Correo, Password, RolUsuario, Activo) 
                             VALUES (@nombre, @correo, '123456', @rol, TRUE);
                             SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nombre", nombreCompleto);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@rol", (byte)rol);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<JustificacionReporteItem>> ObtenerJustificacionesAsync()
        {
            var lista = new List<JustificacionReporteItem>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string initTableQuery = @"
                CREATE TABLE IF NOT EXISTS JUSTIFICACIONES (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    UsuarioId INT NOT NULL,
                    FechaAusencia DATE NOT NULL,
                    Motivo VARCHAR(255) NOT NULL,
                    NombreArchivo VARCHAR(255) NOT NULL,
                    RutaArchivo VARCHAR(500) NOT NULL,
                    FechaSubida DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                );";
            using (var initCmd = new MySqlCommand(initTableQuery, conn))
            {
                await initCmd.ExecuteNonQueryAsync();
            }

            string[] columnasNuevas = new[]
            {
                "ALTER TABLE JUSTIFICACIONES ADD COLUMN Estado INT DEFAULT 0;",
                "ALTER TABLE JUSTIFICACIONES ADD COLUMN ComentarioRRHH VARCHAR(255) NULL;",
                "ALTER TABLE JUSTIFICACIONES ADD COLUMN FechaRevision DATETIME NULL;"
            };

            foreach (var alterSql in columnasNuevas)
            {
                try
                {
                    using var alterCmd = new MySqlCommand(alterSql, conn);
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex) when (ex.Number == 1060)
                {
                }
                catch
                {
                }
            }

            string query = @"SELECT j.Id, u.NombreCompleto, u.Correo, j.FechaAusencia, j.Motivo, 
                                    j.NombreArchivo, j.RutaArchivo, j.FechaSubida, 
                                    IFNULL(j.Estado, 0) AS Estado, j.ComentarioRRHH, j.FechaRevision 
                             FROM JUSTIFICACIONES j 
                             INNER JOIN Usuarios u ON j.UsuarioId = u.Id 
                             ORDER BY j.FechaSubida DESC";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new JustificacionReporteItem
                {
                    Id = reader.GetInt32("Id"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    Correo = reader.GetString("Correo"),
                    FechaAusencia = reader.GetDateTime("FechaAusencia"),
                    Motivo = reader.GetString("Motivo"),
                    NombreArchivo = reader.GetString("NombreArchivo"),
                    RutaArchivo = reader.GetString("RutaArchivo"),
                    FechaSubida = reader.GetDateTime("FechaSubida"),
                    Estado = reader.GetInt32("Estado"),
                    ComentarioRRHH = reader.IsDBNull(reader.GetOrdinal("ComentarioRRHH")) ? null : reader.GetString("ComentarioRRHH"),
                    FechaRevision = reader.IsDBNull(reader.GetOrdinal("FechaRevision")) ? null : reader.GetDateTime("FechaRevision")
                });
            }
            return lista;
        }

        public async Task<bool> ActualizarEstadoJustificacionAsync(int id, int nuevoEstado, string? comentario)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE JUSTIFICACIONES 
                             SET Estado = @estado, ComentarioRRHH = @comentario, FechaRevision = @fecha 
                             WHERE Id = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@estado", nuevoEstado);
            cmd.Parameters.AddWithValue("@comentario", (object?)comentario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", id);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DesactivarUsuarioAsync(int usuarioId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "UPDATE Usuarios SET Activo = FALSE WHERE Id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", usuarioId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}