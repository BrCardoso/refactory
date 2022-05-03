using BeneficiaryAppService.Converters;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Repository.Interfaces;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons.Base;
using Commons.Enums;
using Commons.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class BenefitService : IBenefitService
    {
        private readonly IConfiguration _config;
        private readonly IFamilyRepository _familyRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IEmployeeRequestService _employeeRequest;
        private readonly INITService _nITService;
        private readonly IEventRequestService _eventRequestService;
        public BenefitService(IConfiguration configuration, IFamilyRepository familyRepository, IPersonRepository personRepository, IEmployeeRequestService employeeRequest, INITService nITService, IEventRequestService eventRequestService)
        {
            _config = configuration;
            _familyRepository = familyRepository;
            _personRepository = personRepository;
            _employeeRequest = employeeRequest;
            _nITService = nITService;
            _eventRequestService = eventRequestService;
        }

        public async Task<FamilyFull> blockBenefitAsync(BenefitTransactionModel.BenefitDocument model, string authorization)
        {
            try
            {
                var foundDocument = await _familyRepository.FindByFamilyGuidAsync(model.guid);
                if (!(foundDocument is FamilyFull))
                    return new FamilyFull { HttpStatusCode = 500, Success = false, Message = "Não foi encontrado nenhum documento" };

                var providerGuid = model.family[0].benefitinfos[0].providerguid;
                var productCode = model.family[0].benefitinfos[0].productcode;
                var contractNumber = model.family[0].benefitinfos[0].contractnumber;

                var newBlockDate = model.family[0].benefitinfos[0].blockdate;
                var newBlockReason = model.family[0].benefitinfos[0].BlockReason;

                var beneficiary = foundDocument.family.FirstOrDefault(f => f.personguid == model.family[0].personguid);
                List<bool> op = new List<bool>();
                if (beneficiary.Typeuser.ToUpper() == "TITULAR")
                {
                    foreach (var ben in foundDocument.family)
                    {
                        var benefit = ben.Benefitinfos?.First(b => b.providerguid == providerGuid
                           && b.productcode == productCode
                           && b.contractnumber == contractNumber
                           && string.IsNullOrEmpty(b.BlockReason));
                        if (benefit != null)
                            op.Add(await BlockAsync(benefit));
                    }
                }
                else
                {
                    var benefit = beneficiary.Benefitinfos?.FirstOrDefault(b => b.providerguid == providerGuid
                        && b.productcode == productCode
                        && b.contractnumber == contractNumber
                        && string.IsNullOrEmpty(b.BlockReason));

                    if (benefit != null)
                        op.Add(await BlockAsync(benefit));
                }

                if (op.Count == 1 && op[0] == false)
                    return new FamilyFull { HttpStatusCode = 500, Success = false, Message = "O beneficio informado ja está bloqueado" };
                if (op.Where(x => x == false).ToList().Count > 0)
                    return new FamilyFull { HttpStatusCode = 500, Success = false, Message = "O beneficio informado ja está bloqueado" };

                foundDocument.HttpStatusCode = 200;
                return foundDocument;

                async Task<bool> BlockAsync(Benefitinfo benefit)
                {
                    if (benefit.blockdate != null)
                        return false;

                    benefit.blockdate = newBlockDate;
                    benefit.BlockReason = newBlockReason;

                    var index = foundDocument.family
                        .FindIndex(f => f.personguid == model.family[0].personguid);

                    var index2 = foundDocument.family[index].Benefitinfos
                        .FindIndex(b => b.providerguid == providerGuid && b.productcode == productCode && b.contractnumber == contractNumber);

                    foundDocument.family[index].Benefitinfos[index2] = benefit;

                    if (!string.IsNullOrEmpty(model.filetype))
                    {
                        var updDocument = _personRepository.UpSertDocumentsAsync(
                            model.family[0].personguid,
                            new List<Commons.Document>(){
                        new Commons.Document{
                            type = model.filetype, image_front = model.file, incdate = DateTime.Now
                            }
                                });
                    }
                    var foundPerson = await _personRepository.FindByPersonGuidAsync(model.family[0].personguid);
                    var foundEmployee = await _personRepository.FindByPersonGuidAsync(foundDocument.personguid);
                    var foundEmployeeInfo = await _employeeRequest.getInfo(model.hubguid, model.aggregator, foundDocument.personguid, authorization);
                    var updtFamilyobj = await _familyRepository.UpSert((FamilyDb)foundDocument);

                    var sendToNIT = await _nITService.SendAsync(
                        model.hubguid,
                        model.aggregator,
                        foundDocument.family[index].Benefitinfos[index2],
                        MovimentTypeEnum.EXCLUSÃO,
                        foundPerson,
                        foundEmployee,
                        foundEmployeeInfo,
                        foundDocument.family[index].Typeuser,
                        foundDocument.family[index].Kinship,
                        authorization);

                    _ = _eventRequestService.CreateEventAsync(BenefityToEventConverter.Parse(foundDocument, new BlockEvent
                    {
                        BlockDate = benefit.blockdate,
                        BlockReason = benefit.BlockReason,
                        providerGuid = providerGuid,
                        providerProductCode = productCode,
                        personGuid = model.family[0].personguid,
                        personName = foundPerson.Name,
                        providerName = model.family[0].benefitinfos[0].providerName,
                        Product = model.family[0].benefitinfos[0].product
                    }), authorization);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return new FamilyFull { HttpStatusCode = 500, Success = false, Message = ex.ToString() };
            }
        }
    }
}
