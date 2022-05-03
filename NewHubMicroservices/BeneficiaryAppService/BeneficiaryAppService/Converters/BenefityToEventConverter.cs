using BeneficiaryAppService.Models;
using Commons.Base;
using Commons.Models;
using System.Collections.Generic;

namespace BeneficiaryAppService.Converters
{
    public class BenefityToEventConverter
	{
		public static Event Parse(FamilyFull benefit, BlockEvent blockEvent)
		{
			return new Event
			{
				aggregator = benefit.aggregator,
				hubguid = benefit.hubguid,
				docType = "Events",
				blocks = new List<BlockEvent>
				{
					blockEvent
				}
			};
		}
		public static Event Parse(CardReissue card, Copy2ndRequestEvent copy2Nd)
		{
			return new Event
			{
				aggregator = card.aggregator,
				hubguid = card.hubguid,
				docType = "Events",
				copy2ndRequest = new List<Copy2ndRequestEvent>
                {
					copy2Nd
				}
			};
		}
		public static Event Parse(FamilyFull card, TransferenceEvent transf)
		{
			return new Event
			{
				aggregator = card.aggregator,
				hubguid = card.hubguid,
				docType = "Events",
				transferences = new List<TransferenceEvent>
				{
					transf
				}
			};
		}
	}
}
