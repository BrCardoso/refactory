using Commons;
using System;
using System.Collections.Generic;

namespace chargeAppServices.Models
{
    public class CustomerFull
    {
        public class HuBCustomerModel
        {
            //[NotEmpty]
            public Guid guid { get; set; }
            //[NotEmpty]
            public string contractNumber { get; set; }
            //[NotEmpty]
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
            //[NotEmpty]
            public string Name { get; set; }
            //[NotEmpty]
            public string Cpf { get; set; }
            //[NotEmpty]
            public DateTime? BirthDate { get; set; }
            //[NotEmpty]
            public List<string> Section { get; set; }
            public List<Phoneinfo> phoneinfos { get; set; }
            public List<Emailinfo> emailinfos { get; set; }
        }

        public class CompanyStruCB
        {
            //[NotEmpty]
            public Guid companyguid { get; set; }
            public string aggregator { get; set; }
            public string GroupName { get; set; }
            public string branchName { get; set; }
        }

        public partial class Hierarchy
        {
            public List<Group> Groups { get; set; }
        }

        public partial class Group
        {
            public string Code { get; set; }
            public List<GroupCompany> Companies { get; set; }
            public string Name { get; set; }
        }

        public partial class GroupCompany
        {
            public List<string> Branches { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string TradingName { get; set; }
        }
    }
}
