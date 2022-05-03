using System;

namespace NetCoreJobsMicroservice.Extensions
{
	public static class AgeByBirthDateExtension
	{
		public static int CalculateAge(this DateTime birthDate)
		{
			DateTime today = DateTime.Today;

			int byToday = (((today.Year * 100) + today.Month) * 100) + today.Day;
			int byBirthDate = (((birthDate.Year * 100) + birthDate.Month) * 100) + birthDate.Day;

			return (byToday - byBirthDate) / 10000;
		}
	}
}