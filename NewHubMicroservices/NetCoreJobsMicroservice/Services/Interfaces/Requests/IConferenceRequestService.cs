using Couchbase.Core;
using NetCoreJobsMicroservice.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
	public interface IConferenceRequestService
	{
		Task<List<FamilyHub>> GetFemiliesAsync(Guid hubguid, string aggregator, Guid providerguid, IEnumerable<string> contracts, string authorization);
		Task<FamilyHub> GetFamilyByCardNumberAsync(Guid hubguid, string cardNumber, string aggregator, string authorization);
		Task<FamilyHub> GetFamilyByCPFAndBirthAsync(Guid hubguid, string cpf, DateTime birthdate, string aggregator, string authorization);
		Task<ConferenceEmploees> GetSalariesAsync(Guid hubguid, string aggregator, string authorization);

		Task<ConferencePriceTableName> GetProductPriceTableAsync(Guid hubguid, Guid providerguid, string contract, IBucket _bucket);

		Task<ConferenceValues.Range> GetValuesAsync(Guid hubguid, Guid providerguid, string productCode, string productPriceTable, double? salary, int age, string aggregator, string authorization);


	}
}