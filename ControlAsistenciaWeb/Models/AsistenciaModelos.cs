using System;

namespace ControlAsistenciaWeb.Models
{
    public enum Rol { empleado = 0, administrador = 1 }
    public enum TipoRegistro { entrada = 0, salida = 1 }

    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Rol RolUsuario { get; set; }
        public string? DescriptorFacial { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class RegistroAsistencia
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaHora { get; set; }
        public TipoRegistro Tipo { get; set; }
        public string MetodoVerificacion { get; set; } = "Facial";
    }

    public class MarcajeRequest
    {
        public int UsuarioId { get; set; }
        public TipoRegistro Tipo { get; set; }
    }

    public class GuardarRostroRequest
    {
        public int UsuarioId { get; set; }
        public string DescriptorFacial { get; set; } = string.Empty;
    }

    public class ReporteAtrasoItem
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaHoraEntrada { get; set; }
        public TimeSpan HoraPactada { get; set; }
        public int MinutosAtraso { get; set; }
    }

    public class ReporteSalidaAnticipadaItem
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaHoraSalida { get; set; }
        public TimeSpan HoraPactada { get; set; }
        public int MinutosAnticipacion { get; set; }
    }

    public class ReporteInasistenciaItem
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class HistorialTrabajadorItem
    {
        public DateTime FechaHora { get; set; }
        public TipoRegistro Tipo { get; set; }
        public string Estado { get; set; } = "Normal";
    }

    public class Justificacion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaAusencia { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
    }


    public class JustificacionReporteItem
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaAusencia { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public int Estado { get; set; } = 0; 
        public string? ComentarioRRHH { get; set; }
        public DateTime? FechaRevision { get; set; }
    }
}