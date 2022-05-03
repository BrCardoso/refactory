using System.Collections.Generic;

namespace IdentityService.Data.Converters.Interfaces
{
	public interface IParserList<TOrigin, TDestiny>
	{
		IEnumerable<TDestiny> Parse(IEnumerable<TOrigin> origin);
	}
}