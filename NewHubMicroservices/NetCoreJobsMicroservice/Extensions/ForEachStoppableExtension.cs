using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Extensions
{
	public static class ForEachStoppableExtension
	{
		public static void ForEachStoppable<T>(this List<T> input, Func<T, bool> action)
		{
			foreach (T t in input)
				if (!action(t))
					break;
		}

		public static void ForEachStoppable<T>(this IEnumerable<T> input, Func<T, bool> action)
		{
			foreach (T t in input)
				if (!action(t))
					break;
		}

		public static async Task ForEachStoppableAsync<T>(this IEnumerable<T> input, Func<T, Task<bool>> action)
		{
			foreach (T t in input)
				if (!await action(t))
					break;
		}
	}
}