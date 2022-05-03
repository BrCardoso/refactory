using NetCoreJobsMicroservice.Models;

using System.Collections.Generic;
using System.Linq;

namespace NetCoreJobsMicroservice.Converters
{
	public class CollectionFamilyToFamilyDBConverter
	{
		public static List<FamilyDB> Parse(IEnumerable<FamilyHub> families)
		{
			var familyDBCollection = new List<FamilyDB>();

			families.All(f =>
			{
				familyDBCollection.Add(new FamilyDB
				{
					aggregator = f.aggregator,
					hubguid = f.hubguid,
					personguid = f.personguid,
					guid = f.guid,
					family = Parse(f.members)
				});

				return true;
			});

			return familyDBCollection;
		}

		private static List<BenefitiaryDB> Parse(List<FamilyHub.Member> benefitiaries)
		{
			var benefitiariesDB = new List<BenefitiaryDB>();

			benefitiaries.ForEach(b =>
			{
				benefitiariesDB.Add(new BenefitiaryDB
				{
					blockDate = b.blockDate,
					blockReason = b.blockReason,
					kinship = b.kinship,
					origin = b.origin,
					personguid = b.personguid,
					sequencial = b.sequencial,
					typeuser = b.typeuser,
					benefitinfos = b.benefitinfos
				});
			});

			return benefitiariesDB;
		}
	}
}