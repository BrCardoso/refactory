using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

using NetCoreJobsMicroservice.Converters;
using NetCoreJobsMicroservice.Extensions;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Repository.Interfaces;
using NetCoreJobsMicroservice.Services.Interfaces;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services
{
	public class ConferenceService : IConferenceService
	{
		private readonly IConferenceRequestService _conferenceRequestService;
		private readonly IConferenceRepository _conferenceRepository;
		private readonly IFamilyRepository _familyRepository;
		private readonly IConferenceFinishService _conferenceFinishService;
		private readonly IConferenceCompareBenefitiaryService _benefitiaryService;
		private readonly IBucket _bucket;
		private readonly IConfiguration _config;
		private readonly ITaskPanelRepository _taskPanelRepository;

		public ConferenceService(IConferenceRequestService conferenceRequestService, IConferenceRepository conferenceRepository, IFamilyRepository familyRepository, IConferenceFinishService conferenceFinishService, IConferenceCompareBenefitiaryService benefitiaryService, IBucketProvider bucket, IConfiguration config, ITaskPanelRepository taskPanelRepository)
		{
			_conferenceRequestService = conferenceRequestService;
			_conferenceRepository = conferenceRepository;
			_familyRepository = familyRepository;
			_conferenceFinishService = conferenceFinishService;
			_benefitiaryService = benefitiaryService;
			_bucket = bucket.GetBucket("DataBucket001");
			_config = config;
			_taskPanelRepository = taskPanelRepository;
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.Error>> CompareAsync(ConferenceDB.Conference<ConferenceDB.Error> conference, string aggregator, string authorization)
		{
			var stopValidation = new HashSet<string>();

			var families = (await _conferenceRequestService.GetFemiliesAsync(conference.hubguid, conference.aggregator, conference.providerguid, conference.contracts, authorization)).ToList();

			RemoveMembersWithoutBenefity(families);

			//REGISTRO SOMENTE NO PROVEDOR
			if (!(families?.Any(x => x.aggregator == conference.aggregator && x.hubguid == conference.hubguid && x.members.Any(f => f.benefitinfos.Any(b => b.providerguid == conference.providerguid)))).Value)
			{
				conference.invoiceDetails.ForEach(x =>
				{
					if (x.errorList.Any(x => x.type == ConferenceErrors.ErrorOnlyProvider || x.type == ConferenceErrors.ErrorOnlyHub))
						return;

					x.errorList.Add(new ConferenceDB.ErrorRequired
					{
						providervalue = x.value.ToString(),
						type = ConferenceErrors.ErrorOnlyProvider
					});
				});

				return conference;
			}

			_benefitiaryService.FindBenefitiaries(families, conference);

			//REGISTRO SOMENTE NO PROVEDOR (caso nao encontre os dados dentro das familias retornadas)
			await conference.invoiceDetails.Where(beneficiary => beneficiary.Provider == null).ForEachStoppableAsync(async beneficiary =>
			{
				if (beneficiary.errorList.Any(x => x.type == ConferenceErrors.ErrorOnlyProvider || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				FamilyHub family = null;
				if (await _conferenceRequestService.GetFamilyByCardNumberAsync(conference.hubguid, beneficiary.cardnumber, aggregator, authorization) is FamilyHub familyDB)
					family = familyDB;
				else if (await _conferenceRequestService.GetFamilyByCPFAndBirthAsync(conference.hubguid, beneficiary.cpf, beneficiary.birthdate, aggregator, authorization) is FamilyHub familyByBirthDB)
					family = familyByBirthDB;

				if (!(family is null))
				{
					_benefitiaryService.FindBenefitiary(family, beneficiary, conference.providerguid);
					families.Add(family);
					return true;
				}

				stopValidation.Add(beneficiary.cardnumber);

				beneficiary.errorList.Add(new ConferenceDB.ErrorRequired
				{
					providervalue = beneficiary.value.ToString(),
					type = ConferenceErrors.ErrorOnlyProvider
				});

				return true;
			});

			//DIVERGENCIA DE CONTRATO
			conference.invoiceDetails.Where(x => x.contract != x.Provider?.contract && x.Provider != null).ForEachStoppable(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorContractDivergence || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.contract,
					providervalue = bi.Provider.contract,
					type = ConferenceErrors.ErrorContractDivergence
				});

				return true;
			});

			//DIVERGENCIA DE PLANO
			conference.invoiceDetails.Where(x => x.productcode != x.Provider?.productcode && x.Provider != null).ForEachStoppable(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorPlan || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.productcode,
					providervalue = bi.Provider.productcode,
					type = ConferenceErrors.ErrorPlan
				});

				return true;
			});

			//ERRO DE VALOR
			ConferenceEmploees salaries = await _conferenceRequestService.GetSalariesAsync(conference.hubguid, aggregator, authorization);
			if (!(salaries is null))
				await conference.invoiceDetails.ForEachStoppableAsync(async benefitiary =>
				{
					if (benefitiary.errorList.Any(x => x.type == ConferenceErrors.ErrorValue || x.type == ConferenceErrors.ErrorOnlyProvider || x.type == ConferenceErrors.ErrorOnlyHub))
						return true;

					if (stopValidation.Contains(benefitiary.cardnumber))
						return true;

					var values = await GetValues(benefitiary);

					if (values is null)
						values = new ConferenceValues.Range { employeevalue = 0, relativevalue = 0, householdvalue = 0 };

					if (benefitiary.typeuser.ToUpper() == "TITULAR")
					{
						if (benefitiary.value == values.employeevalue)
							return true;
						benefitiary.errorList.Add(new ConferenceDB.ErrorRequired
						{
							hubvalue = values.employeevalue.ToString(),
							providervalue = benefitiary.value.ToString(),
							type = ConferenceErrors.ErrorValue
						});
						benefitiary.value = values.employeevalue;
						return true;
					}

					if (benefitiary.typeuser.ToUpper() == "DEPENDENTE")
					{
						if (benefitiary.value == values.relativevalue)
							return true;
						benefitiary.errorList.Add(new ConferenceDB.ErrorRequired
						{
							hubvalue = values.relativevalue.ToString(),
							providervalue = benefitiary.value.ToString(),
							type = ConferenceErrors.ErrorValue
						});
						benefitiary.value = values.relativevalue;
						return true;
					}

					if (benefitiary.typeuser.ToUpper() == "AGREGADO")
					{
						if (benefitiary.value == values.householdvalue)
							return true;
						benefitiary.errorList.Add(new ConferenceDB.ErrorRequired
						{
							hubvalue = values.householdvalue.ToString(),
							providervalue = benefitiary.value.ToString(),
							type = ConferenceErrors.ErrorValue
						});
						benefitiary.value = values.householdvalue;
						return true;
					}

					return true;
				});

			//COBRANCA RETROATIVA
			conference.invoiceDetails.ToList().Where(x => conference.refdate != x.refdate && x.Provider != null).All(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorRetroactiveCollection || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					providervalue = bi.Provider.value.ToString(),
					type = ConferenceErrors.ErrorRetroactiveCollection
				});

				conference.invoiceDetails.Where(x => x.PersonGuid == bi.PersonGuid && x.errorList.Any(f => f.type == ConferenceErrors.ErrorValue) && !x.errorList.Any(a => a.type == ConferenceErrors.ErrorRetroactiveCollection)).ToList().ForEach(x =>
				{
					conference.invoiceDetails.Remove(x);
				});

				return true;
			});

			//COBRANCA DUPLICADA
			conference.invoiceDetails.ToList().ForEach(bi =>
			{
				if (stopValidation.Contains(bi.cardnumber))
					return;

				if (conference.invoiceDetails.Count(a => a.PersonGuid == bi.PersonGuid && a.refdate == bi.refdate) > 1)
				{
					if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorDuplicateCollection || x.type == ConferenceErrors.ErrorOnlyHub))
						return;

					conference.invoiceDetails.Where(duplicate => duplicate.refdate == bi.refdate && (duplicate.PersonGuid == bi.PersonGuid) && duplicate != bi).ToList().ForEach(currentBeneficiaryDuplicate =>
					{
						conference.invoiceDetails.Remove(currentBeneficiaryDuplicate);
					});

					bi.errorList.Add(new ConferenceDB.ErrorRequired
					{
						providervalue = bi.value.ToString(),
						type = ConferenceErrors.ErrorDuplicateCollection
					});
				}
			});

			//DIVERGENCIA DE CARTEIRINHA
			conference.invoiceDetails.Where(x => x.cardnumber != x.Provider?.cardnumber && x.Provider != null).ForEachStoppable(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorCard || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.cardnumber,
					providervalue = bi.Provider.cardnumber,
					type = ConferenceErrors.ErrorCard
				});

				return true;
			});

			//DIVERGENCIA DE DATA DE NASCIMENTO
			conference.invoiceDetails.Where(x => x.birthdate.ToShortDateString() != x.Provider?.birthdate.ToShortDateString() && x.Provider != null).ForEachStoppable(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorBirthDate || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.birthdate.ToShortDateString(),
					providervalue = bi.Provider.birthdate.ToShortDateString(),
					type = ConferenceErrors.ErrorBirthDate
				});
				return true;
			});

			//DIVERGENCIA DE CPF
			conference.invoiceDetails.Where(bi => bi.cpf != bi.Provider?.cpf && bi.Provider != null).All(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorCPF || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.cpf,
					providervalue = bi.Provider.cpf,
					type = ConferenceErrors.ErrorCPF
				});
				return true;
			});

			//DIVERGENCIA DE NAME
			conference.invoiceDetails.Where(bi => bi.name != bi.Provider?.name && bi.Provider != null).All(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorName || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = bi.name,
					providervalue = bi.Provider.name,
					type = ConferenceErrors.ErrorName
				});
				return true;
			});

			//BENEFICIARIO BLOQUEADO
			conference.invoiceDetails.Where(bi => families.Any(fs => fs.members.Any(f => f.blockDate != null && f.benefitinfos.Any(b => b.cardnumber == bi.cardnumber && bi.productcode == b.productcode && b.contractnumber == bi.contract && b.providerguid == conference.providerguid)))).All(bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorBeneficiaryBlock || x.type == ConferenceErrors.ErrorOnlyHub))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				var family = families.SingleOrDefault(f => f.members.Any(x => x.personguid == bi.PersonGuid));
				var member = family?.members.SingleOrDefault(x => x.personguid == bi.PersonGuid);

				bi.errorList.Add(new ConferenceDB.ErrorRequired
				{
					hubvalue = $"{member?.blockDate.Value.ToShortDateString()} - {member?.blockReason}",
					providervalue = bi.Provider.value.ToString(),
					type = ConferenceErrors.ErrorBeneficiaryBlock
				});
				return true;
			});

			// REGISTRO SOMENTE NO HUB
			await families.ForEachStoppableAsync(async family =>
			{
				await family.members.Where(member => !conference.invoiceDetails.Any(x => member.personguid == x.PersonGuid)).ToList().ForEachStoppableAsync(async member =>
			{
				string cardnumber = member.benefitinfos.FirstOrDefault(x => conference.providerguid == x.providerguid)?.cardnumber;
				string contract = member.benefitinfos.FirstOrDefault(x => conference.providerguid == x.providerguid && x.cardnumber == cardnumber)?.contractnumber;
				string productcode = member.benefitinfos.FirstOrDefault(x => conference.providerguid == x.providerguid && x.contractnumber == contract && x.cardnumber == cardnumber)?.productcode;
				var benefitiaryInHub = new ConferenceDB.InvoiceDetail<ConferenceDB.Error>
				{
					birthdate = member.birthdate,
					typeuser = member.typeuser,
					cardnumber = cardnumber,
					cpf = member.cpf,
					name = member.name,
					contract = contract,
					productcode = productcode,
					refdate = conference.refdate,
					PersonGuid = member.personguid
				};

				await SetOwnerShipAndValue();

				conference.invoiceDetails.Add(benefitiaryInHub);

				return true;

				async Task SetOwnerShipAndValue()
				{
					benefitiaryInHub.ownership = GetOwnerShip(benefitiaryInHub.PersonGuid.Value);
					var values = await GetValues(benefitiaryInHub);

					if (benefitiaryInHub.typeuser.ToUpper() == "TITULAR")
					{
						benefitiaryInHub.ownership = new ConferenceDB.OwnerShip
						{
							cardnumber = benefitiaryInHub.cardnumber,
							cpf = benefitiaryInHub.cpf,
							name = benefitiaryInHub.name
						};

						if (!(values is null))
							benefitiaryInHub.value = values.employeevalue;
					}

					if (benefitiaryInHub.typeuser.ToUpper() == "AGREGADO")
					{
						if (!(values is null))
							benefitiaryInHub.value = values.householdvalue;
					}

					if (benefitiaryInHub.typeuser.ToUpper() == "DEPENDENTE")
					{
						if (!(values is null))
							benefitiaryInHub.value = values.relativevalue;
					}

					benefitiaryInHub.errorList = new List<ConferenceDB.Error> {
						new ConferenceDB.Error
						{
							hubvalue = benefitiaryInHub.value.ToString("#.##"),
							type = ConferenceErrors.ErrorOnlyHub
						}};
				}

				ConferenceDB.OwnerShip GetOwnerShip(Guid personguid)
				{
					var family = families.SingleOrDefault(x => x.members.Any(x => x.personguid == personguid));
					var ownershipGuid = family.personguid;
					var familyOwner = family.members.SingleOrDefault(x => x.personguid == ownershipGuid);
					var benefity = familyOwner.benefitinfos.FirstOrDefault(x => x.cardnumber != "");

					var ownershipData = new ConferenceDB.OwnerShip
					{
						cpf = familyOwner.cpf,
						name = familyOwner.name,
						cardnumber = benefity?.cardnumber ?? ""
					};

					return ownershipData;
				}
			});
				return true;
			});

			//BENEFICIARIO EM OUTRA UNIDADE
			await conference.invoiceDetails.Where(benefitiary => families.Any(family => family.aggregator != conference.aggregator && family.members.Any(member => member.personguid == benefitiary.PersonGuid))).ForEachStoppableAsync(async bi =>
			{
				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorBeneficiaryOtherUnity || x.type == ConferenceErrors.ErrorContractDivergence))
					return true;

				if (stopValidation.Contains(bi.cardnumber))
					return true;

				var item = new ConferenceDB.ErrorRequired
				{
					type = ConferenceErrors.ErrorBeneficiaryOtherUnity,
				};

				if (bi.errorList.Any(x => x.type == ConferenceErrors.ErrorOnlyHub))
					item.hubvalue = bi.value.ToString();
				else
				{
					if (!(await GetValues(bi) is ConferenceValues.Range value))
						value = new ConferenceValues.Range { employeevalue = 0, relativevalue = 0, householdvalue = 0 };

					double benefitiaryValue = bi.typeuser.ToUpper() switch
					{
						"TITULAR" => value.employeevalue,
						"DEPENDENTE" => value.relativevalue,
						"AGREGADO" => value.householdvalue,
						_ => 0d
					};

					item.hubvalue = value is null ? "0" : $"{benefitiaryValue:#.##}";
					item.providervalue = bi.value.ToString();
				}

				bi.errorList.Add(item);

				return true;
			});

			return conference;

			async Task<ConferenceValues.Range> GetValues(ConferenceDB.InvoiceDetail<ConferenceDB.Error> benefitiary)
			{
				if (benefitiary.PersonGuid is null)
					return null;

				var salary = salaries?.employees?.SingleOrDefault(x => x.personguid == benefitiary.PersonGuid)?.salary;

				var priceTableName = await _conferenceRequestService.GetProductPriceTableAsync(conference.hubguid, conference.providerguid, benefitiary.contract, _bucket);
				if (priceTableName is null)
					return null;

				var product = priceTableName.products.SingleOrDefault(x => x.code == benefitiary.productcode);
				if (product is null)
					return null;

				return await _conferenceRequestService.GetValuesAsync(conference.hubguid, conference.providerguid, benefitiary.productcode, product.productpricetablename, salary, benefitiary.birthdate.CalculateAge(), aggregator, authorization);
			}
		}

		public ConferenceExtractDB.ConferenceExtract NewExtract(ConferenceExtractInput<OperationType> bIExtractInput, ConferenceExtractDB.ConferenceExtract invoiceExtract)
		{
			if (invoiceExtract is null)
				invoiceExtract = new ConferenceExtractDB.ConferenceExtract
				{
					guid = Guid.NewGuid(),
					aggregator = bIExtractInput.aggregator,
					hubguid = bIExtractInput.hubguid,
					providerguid = bIExtractInput.providerguid,
					account = new ConferenceExtractDB.Account { extract = new List<ConferenceExtractDB.Extract>() }
				};

			invoiceExtract.account.currentvalue = bIExtractInput.operation.type.ToUpper() switch
			{
				ConferenceExtractType.Credit => invoiceExtract.account.currentvalue - Math.Abs(bIExtractInput.operation.value),
				ConferenceExtractType.Debit => invoiceExtract.account.currentvalue + Math.Abs(bIExtractInput.operation.value),
				_ => 0
			};

			if (invoiceExtract.account.currentvalue != 0)
				invoiceExtract.account.currentvalue = double.Parse(invoiceExtract.account.currentvalue.ToString("#.##"));

			if (bIExtractInput.operation.value == 0d)
				return invoiceExtract;

			invoiceExtract.account.extract.Add(new ConferenceExtractDB.Extract
			{
				source = new ConferenceExtractDB.Source { date = DateTime.Now, invoicevalidationguid = bIExtractInput.operation.invoicevalidationguid },
				type = bIExtractInput.operation.type.ToUpper(),
				value = Math.Abs(double.Parse(bIExtractInput.operation.value.ToString("#.##"))),
				used = new ConferenceExtractDB.Used { }
			});

			return invoiceExtract;
		}

		public ConferenceExtractDB.ConferenceExtract UseExtracts(ConferenceExtractInput<Operation> bIExtractInput, ConferenceExtractDB.ConferenceExtract invoiceExtract, string aggregator, string authorization)
		{
			invoiceExtract.account.extract.Where(x => x.used.status == false).All(x =>
			{
				x.used = new ConferenceExtractDB.Used
				{
					date = DateTime.Now,
					invoicevalidationguid = bIExtractInput.operation.invoicevalidationguid,
					status = true
				};
				return true;
			});

			if (invoiceExtract.account.currentvalue < 0)
			{
				double diferenceValue = Math.Abs(Math.Abs(invoiceExtract.account.currentvalue) - Math.Abs(bIExtractInput.operation.value));

				if (diferenceValue > 0)
				{
					invoiceExtract.account.extract.Add(new ConferenceExtractDB.Extract
					{
						source = new ConferenceExtractDB.Source { date = DateTime.Now, invoicevalidationguid = bIExtractInput.operation.invoicevalidationguid },
						type = ConferenceExtractType.Credit,
						value = diferenceValue,
						used = new ConferenceExtractDB.Used { }
					});
				}

				invoiceExtract.account.currentvalue = diferenceValue == 0 ? 0 : -diferenceValue;
				return invoiceExtract;
			}

			if (invoiceExtract.account.currentvalue > 0)
			{
				double diferenceValue = Math.Abs(Math.Abs(invoiceExtract.account.currentvalue) - Math.Abs(bIExtractInput.operation.value));

				if (diferenceValue > 0)
				{
					invoiceExtract.account.extract.Add(new ConferenceExtractDB.Extract
					{
						source = new ConferenceExtractDB.Source { date = DateTime.Now, invoicevalidationguid = bIExtractInput.operation.invoicevalidationguid },
						type = ConferenceExtractType.Debit,
						value = diferenceValue,
						used = new ConferenceExtractDB.Used { }
					});
				}

				invoiceExtract.account.currentvalue = +diferenceValue;
				return invoiceExtract;
			}

			return invoiceExtract;
		}

		public async Task<ConferenceExtractDB.ConferenceExtract> FinishSolutionsAsync(ConferenceDB.Conference<ConferenceDB.ErrorRequired> conference, string aggregator, string authorization)
		{
			var contractsAux = new HashSet<string>();
			conference.invoiceDetails.ForEach(x => contractsAux.Add(x.contract));
			var families = await _conferenceRequestService.GetFemiliesAsync(conference.hubguid, conference.aggregator, conference.providerguid, contractsAux, authorization);
			if (families is null)
				return null;

			RemoveMembersWithoutBenefity(families);
			var conferenceNoRequired = ConferenceRequiredToNoRequiredConverter.Parse(conference);
			_benefitiaryService.FindBenefitiaries(families, conferenceNoRequired);

			var invoiceExtract = new ConferenceExtractDB.ConferenceExtract
			{
				guid = Guid.NewGuid(),
				aggregator = conference.aggregator,
				hubguid = conference.hubguid,
				providerguid = conference.providerguid,
				account = new ConferenceExtractDB.Account { extract = new List<ConferenceExtractDB.Extract>() }
			};

			//adicionando o erro de carteirinha no final da lista sempre para evitar bugs de atualizar a carteirinha e os demais itens nao encontrala mais.
			conferenceNoRequired.invoiceDetails.Where(benetitiary => benetitiary.errorList.Any(error => error.type == ConferenceErrors.ErrorCard)).ToList().ForEach(benetitiary =>
			{
				benetitiary.errorList.ToList().ForEach(error =>
				{
					if (error.type == ConferenceErrors.ErrorCard)
					{
						benetitiary.errorList.Remove(error);
						benetitiary.errorList.Add(error);
					}
				});
			});

			conferenceNoRequired.invoiceDetails.ForEach(bi => bi.errorList.ForEach(async e =>
			{
				if (e.hubvalue == "" && e.type != ConferenceErrors.ErrorCard) e.hubvalue = "0.0";
				if (e.providervalue == "") e.providervalue = "0.0";

				switch (e.type)
				{
					//erros que envolvem money
					case ConferenceErrors.ErrorValue:
					{
						if (e.review != ConferenceErrorSolutions.ErrorValueArchiveAndCredit && e.review != ConferenceErrorSolutions.ErrorValueArchiveAndDebit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.hubvalue) - double.Parse(e.providervalue))
							}
						};

						extract.operation.type = e.review switch
						{
							ConferenceErrorSolutions.ErrorValueArchiveAndCredit => ConferenceExtractType.Credit,
							ConferenceErrorSolutions.ErrorValueArchiveAndDebit => ConferenceExtractType.Debit,
							_ => null
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorBeneficiaryBlock:
					{
						if (e.review != ConferenceErrorSolutions.ErrorBeneficiaryBlockUseAndCredit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Credit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.providervalue))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorRetroactiveCollection:
					{
						if (e.review != ConferenceErrorSolutions.ErrorRetroactiveUseAndCredit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Credit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.providervalue))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorDuplicateCollection:
					{
						if (e.review != ConferenceErrorSolutions.ErrorDuplicateUseAndCredit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Credit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.providervalue))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorOnlyProvider:
					{
						if (e.review != ConferenceErrorSolutions.ErrorRegisterProviderUseAndCredit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Credit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.providervalue))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorBeneficiaryOtherUnity:
					{
						if (e.review != ConferenceErrorSolutions.ErrorBeneficiaryOtherUnityUseAndCredit)
							break;
						bool haveHub = double.TryParse(e.hubvalue, out double hubvalue);
						bool haveProvider = double.TryParse(e.providervalue, out double providervalue);

						double value = (haveHub, haveProvider) switch
						{
							(true, true) => Math.Abs(hubvalue - providervalue),
							(true, false) => hubvalue,
							(false, true) => providervalue,
							_ => 0
						};

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Credit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse($"{value:#.##}"))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					case ConferenceErrors.ErrorOnlyHub:
					{
						if (e.review != ConferenceErrorSolutions.ErrorRegisterHubIgnoreAndDebit)
							break;

						var extract = new ConferenceExtractInput<OperationType>
						{
							operation = new OperationType
							{
								type = ConferenceExtractType.Debit,
								invoicevalidationguid = conferenceNoRequired.guid,
								value = Math.Abs(double.Parse(e.hubvalue))
							}
						};

						invoiceExtract = NewExtract(extract, invoiceExtract);

						break;
					}
					//Erros de cadastro
					case ConferenceErrors.ErrorBirthDate:
					case ConferenceErrors.ErrorPlan:
					case ConferenceErrors.ErrorCPF:
					case ConferenceErrors.ErrorName:
					case ConferenceErrors.ErrorContractDivergence:
					{
						if (e.review != ConferenceErrorSolutions.ErrorOpenTaskPanel)
							break;

						var taskPanel = _conferenceFinishService.CreateTaskPanel(families, bi, new TaskPanel
						{
							aggregator = conferenceNoRequired.aggregator,
							hubguid = conferenceNoRequired.hubguid,
							providerguid = conferenceNoRequired.providerguid,
							subject = e.type,
							divergence = new Divergence { newvalue = e.providervalue, oldvalue = e.hubvalue }
						});

						await _taskPanelRepository.AddTaskPanelAsync(taskPanel);

						break;
					}
					//atualizar a carteirinha caso necessario
					case ConferenceErrors.ErrorCard:
					{
						if (e.review != ConferenceErrorSolutions.ErrorCardUpdate)
							break;

						var familydbCollection = CollectionFamilyToFamilyDBConverter.Parse(families);

						FamilyDB family = _conferenceFinishService.UpdateCard(familydbCollection, bi, conference.providerguid);
						if (family is FamilyDB)
							await _familyRepository.UpdatePersonCardNumberAsync(family);

						break;
					}
					default:
						break;
				}
			}));

			if (invoiceExtract.account.currentvalue == 0)
				return null;

			invoiceExtract.account.extract.Clear();

			var extract = new ConferenceExtractInput<OperationType>
			{
				operation = new OperationType
				{
					type = invoiceExtract.account.currentvalue > 0 ? ConferenceExtractType.Debit : ConferenceExtractType.Credit,
					invoicevalidationguid = conference.guid,
					value = Math.Abs(invoiceExtract.account.currentvalue)
				}
			};

			invoiceExtract.account.currentvalue = 0;

			invoiceExtract = NewExtract(extract, invoiceExtract);

			var invoiceExtractDB = await _conferenceRepository.FindExtracsByProviderAsync(conference.hubguid, conference.aggregator, conference.providerguid);
			if (invoiceExtractDB is null)
				invoiceExtractDB = invoiceExtract;
			else
			{
				invoiceExtractDB.account.currentvalue = (invoiceExtract.account.currentvalue > 0 ? ConferenceExtractType.Debit : ConferenceExtractType.Credit) switch
				{
					ConferenceExtractType.Credit => invoiceExtractDB.account.currentvalue - Math.Abs(invoiceExtract.account.currentvalue),
					ConferenceExtractType.Debit => invoiceExtractDB.account.currentvalue + Math.Abs(invoiceExtract.account.currentvalue),
					_ => 0
				};
				invoiceExtractDB.account.extract.AddRange(invoiceExtract.account.extract);
			}

			return await _conferenceRepository.UpsertExtractAsync(invoiceExtractDB);
		}

		private void RemoveMembersWithoutBenefity(List<FamilyHub> families)
		{
			if (!(families is null))
			{
				families.ToList().ForEach(family => family.members.ToList().ForEach(member =>
				{
					if (member.benefitinfos == null || member.benefitinfos.Count == 0)
						family.members.Remove(member);

					if (family.members.Count == 0)
						families.Remove(family);
				}));
			}
		}
	}
}