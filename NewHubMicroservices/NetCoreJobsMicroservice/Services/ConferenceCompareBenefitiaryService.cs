using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreJobsMicroservice.Services
{
	public class ConferenceCompareBenefitiaryService : IConferenceCompareBenefitiaryService
	{
		public void FindBenefitiaries(IEnumerable<FamilyHub> familiesDB, ConferenceDB.Conference<ConferenceDB.Error> conference)
		{
			conference.invoiceDetails.ForEach(benefitiary => familiesDB.Any(family => FindBenefitiary(family, benefitiary, conference.providerguid)));
					}

		public bool FindBenefitiary(FamilyHub family, ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, Guid providerguid)
		{
			var currentMember = new FamilyHub.Member();
			if (family.members.Any(member => member.benefitinfos.Any(benefity =>
			{
				if (benefity.cardnumber == benefitiary.cardnumber && benefity.providerguid == providerguid)
				{
					currentMember = member;
					return true;
				}

				return false;
			})))
			{
				FillHubValuesAsync(benefitiary, currentMember, providerguid);
				return true;
			}

			if (family.members.Any(member =>
			{
				if (member.birthdate.ToShortDateString() == benefitiary.birthdate.ToShortDateString() && member.cpf == benefitiary.cpf && member.benefitinfos.Any(x => x.providerguid == providerguid))
				{
					currentMember = member;
					return true;
				}

				return false;
			}))
			{
				FillHubValuesAsync(benefitiary, currentMember, providerguid);
				return true;
			}

			if (family.members.Any(member =>
			{
				if (member.name == benefitiary.name && member.benefitinfos.Any(x => x.providerguid == providerguid) && (member.birthdate.ToShortDateString() == benefitiary.birthdate.ToShortDateString() || member.cpf == benefitiary.cpf))
				{
					currentMember = member;
					return true;
				}

				return false;
			}))
			{
				FillHubValuesAsync(benefitiary, currentMember, providerguid);
				return true;
			}

			if (family.members.Any(member =>
			{
				if (member.name == benefitiary.name && family.members.Count(m => m.name == member.name) == 1 && member.benefitinfos.Any(x => x.providerguid == providerguid))
				{
					currentMember = member;
					return true;
				}

				return false;
			}))
			{
				FillHubValuesAsync(benefitiary, currentMember, providerguid);
				return true;
			}

			return false;
		}

		private void FillHubValuesAsync(ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary, FamilyHub.Member benefitiaryHub, Guid providerguid)
		{
			benefitiary.Provider = new ConferenceDB.InvoiceDetail<ConferenceDB.Error>.ProviderValue
			{
				birthdate = benefitiary.birthdate,
				cardnumber = benefitiary.cardnumber,
				contract = benefitiary.contract,
				cpf = benefitiary.cpf,
				name = benefitiary.name,
				productcode = benefitiary.productcode,
				value = benefitiary.value
			};

			benefitiary.birthdate = benefitiaryHub.birthdate;
			benefitiary.contract = benefitiaryHub.benefitinfos.FirstOrDefault(x => x.providerguid == providerguid)?.contractnumber;
			//var benefitinfo = benefitiaryHub.benefitinfos.SingleOrDefault(x => x.providerguid == providerguid && benefitiary.contract == x.contractnumber);
			var benefitinfo = benefitiaryHub.benefitinfos.SingleOrDefault(x => x.providerguid == providerguid && benefitiary.contract == x.contractnumber && x.blockdate == null);
			benefitiary.cardnumber = benefitinfo.cardnumber;
			benefitiary.cpf = benefitiaryHub.cpf;
			benefitiary.name = benefitiaryHub.name;
			benefitiary.productcode = benefitiaryHub.benefitinfos.FirstOrDefault(x => x.providerguid == providerguid && benefitiary.contract == x.contractnumber && x.cardnumber == benefitiary.cardnumber)?.productcode;
			benefitiary.PersonGuid = benefitiaryHub.personguid;
		}
	}
}