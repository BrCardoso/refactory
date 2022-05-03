using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Commons.Base;
using Commons.Enums;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class NITService : INITService
    {
        private readonly IConfiguration config;
        private readonly IRulesConfigurationRequestService rulesConfigurationRequest;
        private readonly IProviderRequestService providerRequest;
        private readonly IQueueRequestService queueRequest;
        public NITService(IConfiguration configuration,
                                IProviderRequestService provider,
                                IQueueRequestService queue,
                                IRulesConfigurationRequestService rule
            )
        {
            config = configuration;
            providerRequest = provider;
            queueRequest = queue;
            rulesConfigurationRequest = rule;
        }
        public async Task<MethodFeedback> SendAsync(
            Guid hubguid, string aggregator, Benefitinfo beneficio, MovimentTypeEnum typeMovement, PersonDB findPerson, PersonDB findEmployee, EmployeeInfo findEmployeeInfo,
            string beneficiarioTpUser, string beneficiarioKinship, string authorization)
        {
            RulesConfigurationModel RC = await rulesConfigurationRequest.Get(hubguid, aggregator, beneficio.providerguid, beneficio.contractnumber, authorization);
            if (RC == null)
            {
                return new MethodFeedback
                {
                    Exception = false,
                    Success = false,
                    Message = "Contrato não localizados.",
                    obj = beneficio.providerguid.ToString() + "-" + hubguid.ToString() + "-" + beneficio.contractnumber + "-" + config.GetValue<string>("Endpoints:RulesConfig")

                };
            }
            //localiza dados do provedor
            var findProvider = await providerRequest.GetProvider(beneficio.providerguid, beneficio.productcode);
            if (findProvider == null)
            {
                return new MethodFeedback
                {
                    Exception = false,
                    Success = false,
                    Message = "Provedor não localizados."
                };
            }
            //localiza dados do provedor na empresa
            var findProviderStruc = await providerRequest.GetProviderStruc(hubguid, aggregator, beneficio.providerguid, authorization);
            if (findProviderStruc == null)
            {
                return new MethodFeedback
                {
                    Exception = false,
                    Success = false,
                    Message = "Estrutura do provedor na empresa não localizado."
                };
            }

            QueueModel queue = new QueueModel
            {
                Hubguid = hubguid,
                ProviderGuid = beneficio.providerguid,
                Aggregator = aggregator,
                MovementType = typeMovement,
                Status = "Aberto",
                Incdate = DateTime.Now,
                LinkInformation = new LinkInformation
                {
                    CostumerNumber = findProviderStruc.accesscredentials.Costumernumber,
                    Login = findProviderStruc.accesscredentials.Login,
                    Password = findProviderStruc.accesscredentials.Password,
                    ContractIssued = RC.contractIssued,
                    ContractNumber = beneficio.contractnumber,
                    ResponsiblePerson = RC.hrresponsable.name,
                    ResponsibleId = RC.hrresponsable.CPF,
                    ResponsibleBirthDate = RC.hrresponsable.birthdate
                },
                Beneficiary = new List<QueueBeneficiary>(),
                Charge = null
            };
            QueueBeneficiary qb = new QueueBeneficiary
            {
                personguid = findPerson.Guid,
                TypeUser = beneficiarioTpUser,
                Kinship = beneficiarioKinship,
                Holder = new Holder
                {
                    personguid = findEmployee.Guid,
                    CardNumber = beneficio.cardnumber,
                    Cpf = findEmployee.Cpf,
                    Name = findEmployee.Name,
                    JobInfo = new EmployeeinfoClean
                    {
                        Admissiondate = findEmployeeInfo.Admissiondate,
                        Costcenter = findEmployeeInfo.Costcenter,
                        Costcentercode = findEmployeeInfo.Costcentercode,
                        Department = findEmployeeInfo.Department,
                        Departmentcode = findEmployeeInfo.Departmentcode,
                        Employeecomplementaryinfos = findEmployeeInfo.Employeecomplementaryinfos,
                        Functionalcategory = findEmployeeInfo.Functionalcategory,
                        Functionalcategorycode = findEmployeeInfo.Functionalcategorycode,
                        Occupation = findEmployeeInfo.Occupation,
                        Occupationcode = findEmployeeInfo.Occupationcode,
                        Registration = findEmployeeInfo.Registration,
                        Role = findEmployeeInfo.Role,
                        Rolecode = findEmployeeInfo.Rolecode,
                        Salary = findEmployeeInfo.Salary,
                        Shift = findEmployeeInfo.Shift,
                        Union = findEmployeeInfo.Union,
                        Unioncode = findEmployeeInfo.Unioncode
                    }
                },
                BenefitInfos = new Benefitinfo
                {
                    providerid = beneficio.providerid,
                    providerguid = beneficio.providerguid,
                    product = findProvider.Products[0].Description,
                    productcode = beneficio.productcode,
                    providerproductCode = findProvider.Products[0].Providerproductcode,
                    subsegment = findProvider.Products[0].Subsegcode,
                    cardnumber = beneficio.cardnumber,
                    startdate = DateTime.Now,
                    blockdate = beneficio.blockdate,
                    BlockReason = beneficio.BlockReason,
                    contractnumber = beneficio.contractnumber                    
                }
            };
            queue.Beneficiary.Add(qb);

            //enviar
            return await queueRequest.Post(queue, authorization);
        }

    }
}
