using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIAppService.Model.Request
{

    public class Family
    {
        public Guid guid { get; set; }

        public Guid hubguid { get; set; }
        public Guid personguid { get; set; }
        public string aggregator { get; set; }
        public List<BeneficiaryIn> family { get; set; }

        public class BeneficiaryIn : Beneficiary
        {
            public Guid personguid { get; set; }

            public EmployeeinfoClean employeeinfo { get; set; }

            public class EmployeeinfoClean : IEmployeeinfo
            {
                public string Registration { get; set; }
                public DateTime Admissiondate { get; set; }
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
                public float? Salary { get; set; }
                public List<Complementaryinfo> Employeecomplementaryinfos { get; set; }

            }
        }
    }
}
