using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public partial class HubCustomerOut
    {
        public Guid Guid { get; set; }
        public string ContractNumber { get; set; }
        public string ContractIssued { get; set; }
        public string Status { get; set; }
        public DateTime? Blockdate { get; set; }
        public string Blockreason { get; set; }
        public List<CompanyOut> Companies { get; set; }
        public Hierarchy Hierarchy { get; set; }

        public partial class CompanyOut
        {
            public Guid Companyguid { get; set; }
            public string aggregator { get; set; }
            public string groupName { get; set; }
            public string branchName { get; set; }
            public List<EmployeeOut> Employees { get; set; }
            public List<ProviderOut> Providers { get; set; }

        }
        public partial class ProviderOut
        {
            public Guid Providercustomerguid { get; set; }
        }
        public partial class EmployeeOut
        {
            public Guid Familyguid { get; set; }
            public Guid Personguid { get; set; }
            public string Registration { get; set; }
            public string Admissiondate { get; set; }
            public string Occupation { get; set; }
            public string Occupationcode { get; set; }
            public string Role { get; set; }
            public string Rolecode { get; set; }
            public string Department { get; set; }
            public string Departmentcode { get; set; }
            public string Costcenter { get; set; }
            public string Costcentercode { get; set; }
            public string Union { get; set; }
            public string Unioncode { get; set; }
            public string Functionalcategory { get; set; }
            public string Functionalcategorycode { get; set; }
            public string Shift { get; set; }
            public float Salary { get; set; }
            public List<Complementaryinfo> Employeecomplementaryinfos { get; set; }

            //public Employeeinfo() {
            //    this.Employeecomplementaryinfos = new List<Complementaryinfo> { new Complementaryinfo()};
            //}
        }
    }    
}
