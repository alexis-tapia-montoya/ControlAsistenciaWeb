using Microsoft.AspNetCore.Mvc;
using ControlAsistenciaWeb.Models;
using ControlAsistenciaWeb.Services;

namespace ControlAsistenciaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly DatabaseService _dbService;

        public AsistenciaController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            var usuarios = await _dbService.ObtenerUsuariosActivosAsync();
            return Ok(usuarios);
        }

        [HttpGet("usuarios-rostros")]
        public async Task<IActionResult> ObtenerUsuariosConRostros()
        {
            var usuarios = await _dbService.ObtenerUsuariosConRostroAsync();
            return Ok(usuarios);
        }

        [HttpPost("registrar-rostro")]
        public async Task<IActionResult> RegistrarRostro([FromBody] GuardarRostroRequest request)
        {
            if (request.UsuarioId <= 0 || string.IsNullOrWhiteSpace(request.DescriptorFacial))
                return BadRequest(new { exito = false, mensaje = "Datos biométricos o ID inválidos." });

            bool guardado = await _dbService.GuardarDescriptorFacialAsync(request.UsuarioId, request.DescriptorFacial);
            if (guardado)
                return Ok(new { exito = true, mensaje = "Rostro registrado correctamente en la base de datos." });

            return StatusCode(500, new { exito = false, mensaje = "No se pudo registrar el rostro." });
        }

        [HttpPost("marcar")]
        public async Task<IActionResult> MarcarAsistencia([FromBody] MarcajeRequest request)
        {
            if (request.UsuarioId <= 0)
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido." });

            bool registrado = await _dbService.RegistrarMarcaAsync(request.UsuarioId, request.Tipo);

            if (registrado)
            {
                string tipoTexto = request.Tipo == TipoRegistro.entrada ? "Entrada" : "Salida";
                return Ok(new
                {
                    exito = true,
                    mensaje = $"{tipoTexto} registrada exitosamente a las {DateTime.Now:HH:mm:ss}."
                });
            }

            return StatusCode(500, new { exito = false, mensaje = "Error al registrar la asistencia." });
        }

        [HttpPost("login-facial")]
        public async Task<IActionResult> LoginFacial([FromBody] MarcajeRequest request)
        {
            if (request.UsuarioId <= 0)
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido." });

            var usuarios = await _dbService.ObtenerUsuariosActivosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Id == request.UsuarioId);

            if (usuario == null)
                return NotFound(new { exito = false, mensaje = "Usuario no encontrado o inactivo." });

            string redirectUrl = usuario.RolUsuario == Rol.administrador
                ? "/Admin/Dashboard"
                : $"/Trabajador/Portal?usuarioId={usuario.Id}";

            return Ok(new
            {
                exito = true,
                usuario = usuario.NombreCompleto,
                rol = usuario.RolUsuario.ToString(),
                redirectUrl = redirectUrl
            });
        }
    }
}