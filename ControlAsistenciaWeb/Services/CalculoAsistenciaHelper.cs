using System;

namespace ControlAsistenciaWeb.Services
{
    public static class CalculoAsistenciaHelper
    {
        // Retorna los minutos de atraso respecto a la hora esperada
        public static int CalcularMinutosAtraso(TimeSpan horaMarcaje, TimeSpan horaEntradaEsperada, int toleranciaMinutos = 0)
        {
            TimeSpan horaLimite = horaEntradaEsperada.Add(TimeSpan.FromMinutes(toleranciaMinutos));
            if (horaMarcaje > horaLimite)
            {
                return (int)Math.Floor((horaMarcaje - horaEntradaEsperada).TotalMinutes);
            }
            return 0;
        }

        // Retorna los minutos previos a la hora de salida pactada
        public static int CalcularMinutosSalidaAnticipada(TimeSpan horaMarcaje, TimeSpan horaSalidaEsperada)
        {
            if (horaMarcaje < horaSalidaEsperada)
            {
                return (int)Math.Floor((horaSalidaEsperada - horaMarcaje).TotalMinutes);
            }
            return 0;
        }
    }
}