using Microsoft.AspNetCore.Mvc;
using ControlAsistenciaWeb.Services;
using ControlAsistenciaWeb.Models;

namespace ControlAsistenciaWeb.Controllers
{
    public class TrabajadorController : Controller
    {
        private readonly DatabaseService _dbService;
        private readonly IWebHostEnvironment _env;

        public TrabajadorController(DatabaseService dbService, IWebHostEnvironment env)
        {
            _dbService = dbService;
            _env = env;
        }

        // Pantalla principal del trabajador: /Trabajador/Portal?usuarioId=X
        public async Task<IActionResult> Portal(int usuarioId)
        {
            var usuarios = await _dbService.ObtenerUsuariosActivosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Usuario = usuario;
            ViewBag.Historial = await _dbService.ObtenerHistorialPersonalAsync(usuarioId);

            return View();
        }

        // Acción para marcar Entrada o Salida desde su portal
        [HttpPost]
        public async Task<IActionResult> RegistrarMarca(int usuarioId, int tipo)
        {
            try
            {
                await _dbService.RegistrarMarcaAsync(usuarioId, (TipoRegistro)tipo);
                TempData["Exito"] = tipo == 0 ? "¡Entrada registrada correctamente!" : "¡Salida registrada correctamente!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar asistencia: " + ex.Message;
            }

            return RedirectToAction("Portal", new { usuarioId });
        }

        // Acción para subir justificación o licencia médica
        [HttpPost]
        public async Task<IActionResult> SubirJustificacion(int usuarioId, DateTime fechaAusencia, string motivo, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debe adjuntar un archivo (PDF o Imagen).";
                return RedirectToAction("Portal", new { usuarioId });
            }

            try
            {
                // Crear carpeta uploads si no existe en wwwroot
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar nombre de archivo único
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                string rutaRelativa = $"/uploads/{fileName}";

                await _dbService.GuardarJustificacionAsync(usuarioId, fechaAusencia, motivo, archivo.FileName, rutaRelativa);
                TempData["Exito"] = "Licencia o justificativo enviado con éxito a Recursos Humanos.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al subir documento: " + ex.Message;
            }

            return RedirectToAction("Portal", new { usuarioId });
        }
    }
}