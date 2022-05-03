using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreJobsMicroservice.Services
{
	public class ConferenceFinishService : IConferenceFinishService
	{
		public TaskPanel CreateTaskPanel(IEnumerable<FamilyHub> families, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, TaskPanel taskPanel)
		{
			string hubCardNumber = benefitiary.cardnumber;
			string hubContractNumber = benefitiary.contract;
			string hubProductNumber = benefitiary.productcode;

			var family = families.SingleOrDefault(x => x.members.Any(x => x.benefitinfos.Any(x => x.providerguid == taskPanel.providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber)));
			if (family == null)
				return null;

			var hubBbenefitiary = family.members.SingleOrDefault(x => x.benefitinfos.Any(x => x.providerguid == taskPanel.providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber));

			var benefity = hubBbenefitiary.benefitinfos.SingleOrDefault(x => x.providerguid == taskPanel.providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber);

			taskPanel.aggregator = family.aggregator;
			taskPanel.hubguid = family.hubguid;
			taskPanel.personguid = hubBbenefitiary.personguid;
			taskPanel.status = "ABERTO";
			taskPanel.origin = "BATE FATURA";
			taskPanel.movtype = "BATE FATURA";

			taskPanel.benefitinfos = new Commons.Base.Benefitinfo
			{
				providerguid = taskPanel.providerguid,
				cardnumber = benefity.cardnumber,
				contractnumber = benefity.contractnumber,
				productcode = benefity.productcode,
				providerid = benefity.providerid
			};

			taskPanel.beneficiarydetails = new BeneficiaryDetails
			{
				birthdate = hubBbenefitiary.birthdate,
				cpf = hubBbenefitiary.cpf,
				gender = hubBbenefitiary.gender,
				kinship = hubBbenefitiary.kinship,
				maritalstatus = hubBbenefitiary.maritalstatus,
				name = hubBbenefitiary.name
			};

			return taskPanel;
		}

		public FamilyDB UpdateCard(IEnumerable<FamilyDB> families, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, Guid providerguid)
		{
			string newCardNumber = null;
			string hubCardNumber = null;
			string hubContractNumber = benefitiary.contract;
			string hubProductNumber = benefitiary.productcode;

			benefitiary.errorList.ForEach(x =>
			{
				if (x.type == ConferenceErrors.ErrorCard)
				{
					hubCardNumber = x.hubvalue;
					newCardNumber = x.providervalue;
				}

				if (x.type == ConferenceErrors.ErrorContractDivergence)
					hubContractNumber = x.hubvalue;

				if (x.type == ConferenceErrors.ErrorPlan)
					hubProductNumber = x.hubvalue;
			});

			var family = families.FirstOrDefault(x => x.family.Any(x => x.personguid == benefitiary.PersonGuid && x.benefitinfos.Any(x => x.providerguid == providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber)));
			if (family is null)
				return null;

			var hubBbenefitiary = family.family.SingleOrDefault(x => x.personguid == benefitiary.PersonGuid && x.benefitinfos.Any(x => x.providerguid == providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber));

			var benefity = hubBbenefitiary.benefitinfos.SingleOrDefault(x => x.providerguid == providerguid && x.cardnumber == hubCardNumber && x.contractnumber == hubContractNumber && x.productcode == hubProductNumber);

			benefity.cardnumber = newCardNumber;

			return family;
		}
	}
}