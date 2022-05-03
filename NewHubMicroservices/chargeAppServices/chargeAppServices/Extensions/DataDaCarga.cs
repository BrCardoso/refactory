using System;

namespace ChargeAppServices.Extensions
{
    public static class DataDaCarga
    {
        public static DateTime Get(int Daymonthtocharge)
        {
            var daysDiff = Daymonthtocharge - DateTime.Today.Day;
            if (daysDiff >= 0)
                return DateTime.Today.AddDays(daysDiff);
            else
                return DateTime.Today.AddMonths(1).AddDays(daysDiff);
        }
    }
}
