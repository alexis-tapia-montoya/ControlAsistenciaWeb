using Xunit;
using System;
using ControlAsistenciaWeb.Services;

namespace ControlAsistenciaWeb.Tests
{
    public class CalculoAsistenciaTests
    {
        // 1. Llegada puntual -> 0 minutos de atraso
        [Fact]
        public void CalcularMinutosAtraso_LlegadaPuntual_RetornaCero()
        {
            TimeSpan entradaEsperada = new TimeSpan(9, 0, 0);
            TimeSpan horaMarca = new TimeSpan(8, 55, 0);
            int tolerancia = 5;

            int minutos = CalculoAsistenciaHelper.CalcularMinutosAtraso(horaMarca, entradaEsperada, tolerancia);

            Assert.Equal(0, minutos);
        }

        // 2. Llegada dentro del rango de tolerancia -> 0 minutos de atraso
        [Fact]
        public void CalcularMinutosAtraso_DentroDeTolerancia_RetornaCero()
        {
            TimeSpan entradaEsperada = new TimeSpan(9, 0, 0);
            TimeSpan horaMarca = new TimeSpan(9, 4, 0);
            int tolerancia = 5;

            int minutos = CalculoAsistenciaHelper.CalcularMinutosAtraso(horaMarca, entradaEsperada, tolerancia);

            Assert.Equal(0, minutos);
        }

        // 3. Llegada tardía que supera tolerancia -> calcula minutos exactos
        [Fact]
        public void CalcularMinutosAtraso_LlegadaTardia_CalculaMinutosExactos()
        {
            TimeSpan entradaEsperada = new TimeSpan(9, 0, 0);
            TimeSpan horaMarca = new TimeSpan(9, 25, 0);
            int tolerancia = 5;

            int minutos = CalculoAsistenciaHelper.CalcularMinutosAtraso(horaMarca, entradaEsperada, tolerancia);

            Assert.Equal(25, minutos);
        }

        // 4. Salida antes de hora -> calcula minutos de anticipación
        [Fact]
        public void CalcularMinutosSalidaAnticipada_RetiroTemprano_CalculaDiferencia()
        {
            TimeSpan salidaEsperada = new TimeSpan(18, 0, 0);
            TimeSpan horaMarca = new TimeSpan(17, 15, 0);

            int minutos = CalculoAsistenciaHelper.CalcularMinutosSalidaAnticipada(horaMarca, salidaEsperada);

            Assert.Equal(45, minutos);
        }

        // 5. Salida cumplida o posterior -> 0 minutos de anticipación
        [Fact]
        public void CalcularMinutosSalidaAnticipada_SalidaNormal_RetornaCero()
        {
            TimeSpan salidaEsperada = new TimeSpan(18, 0, 0);
            TimeSpan horaMarca = new TimeSpan(18, 10, 0);

            int minutos = CalculoAsistenciaHelper.CalcularMinutosSalidaAnticipada(horaMarca, salidaEsperada);

            Assert.Equal(0, minutos);
        }
    }
}