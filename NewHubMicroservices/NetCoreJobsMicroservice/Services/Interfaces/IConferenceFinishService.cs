using NetCoreJobsMicroservice.Models;

using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Services.Interfaces
{
	public interface IConferenceFinishService
	{
		TaskPanel CreateTaskPanel(IEnumerable<FamilyHub> families, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, TaskPanel taskPanel);

		FamilyDB UpdateCard(IEnumerable<FamilyDB> families, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, Guid providerguid);
	}
}