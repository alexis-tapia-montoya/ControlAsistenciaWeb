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

    // Modelo para recibir el vector biométrico desde la cámara web
    public class GuardarRostroRequest
    {
        public int UsuarioId { get; set; }
        public string DescriptorFacial { get; set; } = string.Empty;
    }
}