using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository.Interfaces
{
	public interface IConferenceRepository
	{
		Task<ConferenceDB.Conference<ConferenceDB.Error>> FindDocByGuidAndDocTypeAsync(Guid guid, Guid hubguid, string aggregator);

		Task<ConferenceExtractDB.ConferenceExtract> FindExtracsByProviderAsync(Guid hubguid, string aggregator, Guid providerguid);

		Task<ConferenceDB.Conference<ConferenceDB.Error>> FindByRefDateAsync(Guid hubguib, string aggregator, Guid providerguid, string refdate, IEnumerable<string> contracts);

		Task<IEnumerable<ConferenceDB.Conference<ConferenceDB.Error>>> FindAllInvoicesAsync(string aggregator, Guid hubguid);

		Task<ConferenceDB.Conference<ConferenceDB.Error>> FindInvoiceDetailsAsync(Guid guid, Guid hubguid, string aggregator);

		Task<ConferenceExtractDB.ConferenceExtract> UpsertExtractAsync(ConferenceExtractDB.ConferenceExtract conferenceExtract);

		Task<ConferenceDB.Conference<ConferenceDB.ErrorRequired>> UpsertAsync(ConferenceDB.Conference<ConferenceDB.ErrorRequired> dataInvoice);

		Task<ConferenceDB.Conference<ConferenceDB.Error>> UpsertAsync(ConferenceDB.Conference<ConferenceDB.Error> dataInvoice);

		Task<bool> DeleteByGuidAsync(Guid guid, Guid hubguid, string aggregator);
	}
}