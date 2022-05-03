using System;
using System.Collections.Generic;

namespace BeneficiaryAppService.Models
{
    public partial class TaskPanelModel
    {
        public Guid guid { get; set; }

        [NotEmpty]
        public Guid hubguid { get; set; }

        [NotEmpty]
        public string aggregator { get; set; }

        [NotEmpty]
        public Guid personguid { get; set; }

        [NotEmpty]
        public string status { get; set; }

        public DateTime? incdate { get; set; }

        [NotEmpty]
        public string origin { get; set; }

        [NotEmpty]
        public Guid providerguid { get; set; }

        public List<Guid> mandatoryrulesguids { get; set; }

        [NotEmpty]
        public string subject { get; set; }

        public string obs { get; set; }

        public string response { get; set; }

        public DateTime? conclusiondate { get; set; }

        public Divergence divergence { get; set; }

        public string movtype { get; set; }

        public BeneficiaryDetails beneficiarydetails { get; set; }

        public Commons.Base.Benefitinfo benefitinfos { get; set; }
    }

    public partial class BeneficiaryDetails
    {
        public string name { get; set; }

        public string gender { get; set; }

        public DateTime birthdate { get; set; }

        public string cpf { get; set; }

        public string rg { get; set; }

        public string kinship { get; set; }

        public string maritalstatus { get; set; }
    }

    public partial class Divergence
    {
        public string oldvalue { get; set; }

        public string newvalue { get; set; }
    }
}
