using NetCoreJobsMicroservice.Models;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Services.Interfaces
{
	public interface IConferenceCompareBenefitiaryService
	{
		void FindBenefitiaries(IEnumerable<FamilyHub> families, ConferenceDB.Conference<ConferenceDB.Error> conference);

		bool FindBenefitiary(FamilyHub family, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, Guid providerguid);
	}
}