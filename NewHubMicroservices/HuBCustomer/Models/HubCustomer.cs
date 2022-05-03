using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace HuBCustomerAppService.Models
{
    public class HuBCustomerModel
    {
        [NotEmpty]
        public Guid guid { get; set; }
        [NotEmpty]
        public string contractNumber { get; set; }
        [NotEmpty]
        public string contractIssued { get; set; }
       public string status { get; set; }
       public string blockdate { get; set; }
       public string blockreason { get; set; }
        public Hierarchy hierarchy { get; set; }
        public List<CompanyStruCB> companies { get; set; }
        public List<Responsables> responsables { get; set; }
    }

    public class Responsables
    {
        [NotEmpty]
        public string Name { get; set; }
        [NotEmpty]
        public string Cpf { get; set; }
        [NotEmpty]
        public DateTime? BirthDate { get; set; }
        [NotEmpty]
        public List<string> Section { get; set; }
        public List<Phoneinfo> phoneinfos { get; set; }
        public List<Emailinfo> emailinfos { get; set; }
    }

    public class CompanyStruCB
    {
        [NotEmpty]
        public Guid companyguid { get; set; }
        public string aggregator { get; set; }
        public string GroupName { get; set; }
        public string branchName { get; set; }
    }

    public class onboarding
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public string aggregator { get; set; }
        public bool status { get; set; }
        public steps steps { get; set; }

    }

    public class steps
    {
        public bool provider { get; set; }
        public bool benefit { get; set; }
        public bool pricetable { get; set; }
        public bool entity { get; set; }
        public bool rulesconfiguration { get; set; }
        public bool beneficiary { get; set; }

    }
}
