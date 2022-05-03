using System.Collections.Generic;

namespace NotifierAppService.Converters
{
	public class EnumerableStringToUpperCaseConverter
	{
		public static List<string> Parse(IEnumerable<string> values)
		{
			var newValues = new List<string>();

			foreach (var value in values)
				newValues.Add(value.ToUpper());

			return newValues;
		}
	}
}