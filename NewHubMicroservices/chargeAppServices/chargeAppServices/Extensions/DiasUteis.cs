using chargeAppServices.Models;
using System;
using System.Linq;

namespace ChargeAppServices.Extensions
{
    public static class DiasUteis
    {
        public static object Calcula(DateTime dDataCarga, Workingdays oUteis, bool? lDescontaFeriados, Holidays ResFeriados)
        {
            int totalDias = 0;
            DateTime dDataBase = dDataCarga;

            if (dDataCarga.Day <= 15) // se o dia da carga é ate o dia 15, assumido como calculo o mes atual 
            { dDataBase = dDataCarga; }
            else
            { dDataBase = dDataCarga.AddMonths(1); }

            int nDiasNoMes = DateTime.DaysInMonth(dDataBase.Year, dDataBase.Month);

            for (int i = 0; i < nDiasNoMes; i++)
            {
                DateTime DataAtual = dDataBase.AddDays(-(dDataBase.Day) + i + 1);

                if (!Feriado(DataAtual, lDescontaFeriados, ResFeriados))
                {
                    if (DataAtual.DayOfWeek.ToString() == "Sunday") { if (oUteis.Sunday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Monday") { if (oUteis.Monday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Tuesday") { if (oUteis.Tuesday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Wednesday") { if (oUteis.Wednesday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Thursday") { if (oUteis.Thursday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Friday") { if (oUteis.Friday) { totalDias++; } }
                    if (DataAtual.DayOfWeek.ToString() == "Saturday") { if (oUteis.Saturday) { totalDias++; } }
                }
            }
            return totalDias;

            bool Feriado(DateTime dataVerificar, bool? lDescontaFeriados, Holidays ResFeriados)
            {
                if (Convert.ToBoolean(lDescontaFeriados))
                {
                    var feriado = ResFeriados.Holiday.Where(x => x.Data == dataVerificar.ToString("dd/MM/yyyy")).SingleOrDefault();
                    if (feriado != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
    }
}
