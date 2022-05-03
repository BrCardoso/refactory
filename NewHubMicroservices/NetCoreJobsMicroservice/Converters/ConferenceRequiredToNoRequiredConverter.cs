using NetCoreJobsMicroservice.Models;

using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Converters
{
	public class ConferenceRequiredToNoRequiredConverter
	{
		public static ConferenceDB.Conference<ConferenceDB.Error> Parse(ConferenceDB.Conference<ConferenceDB.ErrorRequired> conference)
		{
			return new ConferenceDB.Conference<ConferenceDB.Error>
			{
				contracts = conference.contracts,
				aggregator = conference.aggregator,
				copartvalue = conference.copartvalue,
				dispatch = conference.dispatch,
				docType = conference.docType,
				file = conference.file,
				guid = conference.guid,
				hubguid = conference.hubguid,
				incdate = conference.incdate,
				invvalue = conference.invvalue,
				providerguid = conference.providerguid,
				ratevalue = conference.ratevalue,
				receipt = conference.receipt,
				receiptnumber = conference.receiptnumber,
				refdate = conference.refdate,
				status = conference.status,
				invoiceDetails = ParseDetails(conference.invoiceDetails)
			};
		}

		private static List<ConferenceDB.InvoiceDetail<ConferenceDB.Error>> ParseDetails(List<ConferenceDB.InvoiceDetail<ConferenceDB.ErrorRequired>> details)
		{
			var noRequiredDetails = new List<ConferenceDB.InvoiceDetail<ConferenceDB.Error>>();
			foreach (var detail in details)
			{
				noRequiredDetails.Add(new ConferenceDB.InvoiceDetail<ConferenceDB.Error>
				{
					birthdate = detail.birthdate,
					cardnumber = detail.cardnumber,
					contract = detail.contract,
					cpf = detail.cpf,
					name = detail.name,
					ownership = detail.ownership,
					PersonGuid = detail.PersonGuid,
					productcode = detail.productcode,
					productdescription = detail.productdescription,
					refdate = detail.refdate,
					typeuser = detail.typeuser,
					value = detail.value,
					errorList = ParseErrors(detail.errorList)
				});
			}

			return noRequiredDetails;
		}

		private static List<ConferenceDB.Error> ParseErrors(List<ConferenceDB.ErrorRequired> errors)
		{
			var noRequiredErrors = new List<ConferenceDB.Error>();
			foreach (var error in errors)
			{
				noRequiredErrors.Add(new ConferenceDB.Error
				{
					hubvalue = error.hubvalue,
					providervalue = error.providervalue,
					review = error.review,
					type = error.type
				});
			}

			return noRequiredErrors;
		}
	}
}