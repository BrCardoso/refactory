using NetCoreJobsMicroservice.Models;

using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces
{
	public interface IConferenceService
	{
		Task<ConferenceDB.Conference<ConferenceDB.Error>> CompareAsync(ConferenceDB.Conference<ConferenceDB.Error> conference, string aggregator, string authorization);

		ConferenceExtractDB.ConferenceExtract NewExtract(ConferenceExtractInput<OperationType> bIExtractInput, ConferenceExtractDB.ConferenceExtract invoiceExtract);

		ConferenceExtractDB.ConferenceExtract UseExtracts(ConferenceExtractInput<Operation> bIExtractInput, ConferenceExtractDB.ConferenceExtract invoiceExtract, string aggregator, string authorization);

		Task<ConferenceExtractDB.ConferenceExtract> FinishSolutionsAsync(ConferenceDB.Conference<ConferenceDB.ErrorRequired> conference, string aggregator, string authorization);
	}
}