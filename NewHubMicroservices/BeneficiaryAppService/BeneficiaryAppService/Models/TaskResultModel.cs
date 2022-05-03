using Commons.Base;
using Commons.Enums;
using System;

namespace BeneficiaryAppService.Models
{
    public class TaskResultModel
    {
        public Guid hubguid { get; set; }

        public string aggregator { get; set; }

        public MovimentTypeEnum movType { get; set; }

        public Guid personguid { get; set; }

        public Benefitinfo benefitinfos { get; set; }
    }
}