using Microsoft.AspNetCore.Mvc;
using ControlAsistenciaWeb.Services;

namespace ControlAsistenciaWeb.Controllers
{
    public class ReportesController : Controller
    {
        private readonly DatabaseService _dbService;

        public ReportesController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DateTime inicio = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime fin = fechaHasta ?? DateTime.Today;
            DateTime diaInasistencia = fin;

            TimeSpan horaEntrada = new TimeSpan(9, 0, 0);   // 09:00 hrs
            TimeSpan horaSalida = new TimeSpan(18, 0, 0);   // 18:00 hrs

            ViewBag.FechaDesde = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fin.ToString("yyyy-MM-dd");

            ViewBag.Atrasos = await _dbService.ObtenerAtrasosAsync(inicio, fin, horaEntrada, toleranciaMinutos: 5);
            ViewBag.Salidas = await _dbService.ObtenerSalidasAnticipadasAsync(inicio, fin, horaSalida);
            ViewBag.Inasistencias = await _dbService.ObtenerInasistenciasAsync(diaInasistencia);

            return View();
        }
    }
}