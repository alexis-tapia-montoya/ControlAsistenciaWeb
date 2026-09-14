using Microsoft.AspNetCore.Mvc;
using ControlAsistenciaWeb.Services;
using ControlAsistenciaWeb.Models;

namespace ControlAsistenciaWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly DatabaseService _dbService;

        public AdminController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> Dashboard(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DateTime inicio = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime fin = fechaHasta ?? DateTime.Today;

            TimeSpan horaEntrada = new TimeSpan(9, 30, 0);
            TimeSpan horaSalida = new TimeSpan(17, 30, 0);

            ViewBag.FechaDesde = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fin.ToString("yyyy-MM-dd");

            ViewBag.Atrasos = await _dbService.ObtenerAtrasosAsync(inicio, fin, horaEntrada, toleranciaMinutos: 0);
            ViewBag.Salidas = await _dbService.ObtenerSalidasAnticipadasAsync(inicio, fin, horaSalida);
            ViewBag.Inasistencias = await _dbService.ObtenerInasistenciasAsync(fin);
            ViewBag.Usuarios = await _dbService.ObtenerUsuariosActivosAsync();
            ViewBag.Justificaciones = await _dbService.ObtenerJustificacionesAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(string nombreCompleto, string correo, byte rol)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(correo))
            {
                TempData["Error"] = "Nombre y correo son obligatorios.";
                return RedirectToAction("Dashboard");
            }

            try
            {
                int nuevoId = await _dbService.CrearUsuarioAsync(nombreCompleto, correo, (Rol)rol);
                TempData["Exito"] = $"Usuario '{nombreCompleto}' registrado exitosamente con ID: {nuevoId}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar usuario: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstadoJustificacion(int id, int nuevoEstado, string comentario)
        {
            try
            {
                await _dbService.ActualizarEstadoJustificacionAsync(id, nuevoEstado, comentario);
                TempData["Exito"] = nuevoEstado == 1 ? "Justificación APROBADA con éxito." : "Justificación RECHAZADA.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar estado: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            try
            {
                await _dbService.DesactivarUsuarioAsync(id);
                TempData["Exito"] = "Colaborador dado de baja correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar usuario: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }
    }
}