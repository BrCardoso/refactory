using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class FamilyOut
    {
        public Guid Guid { get; set; }
        public Guid Hubguid { get; set; }
        public string Aggregator { get; set; }
        public List<BeneficiaryOut> Family { get; set; }

        public partial class BeneficiaryOut
        {
            public Guid Personguid { get; set; }
            public string Origin { get; set; }
            public string Sequencial { get; set; }
            public string Kinship { get; set; }
            public string Typeuser { get; set; }
            public DateTime? Blockdate { get; set; }
            public string Blockreason { get; set; }
            public List<Benefitinfo> Benefitinfo { get; set; }

            public static explicit operator BeneficiaryOut(Beneficiary v)
            {
                BeneficiaryOut ben = new BeneficiaryOut
                {
                    Benefitinfo = v.Benefitinfos,
                    Blockdate = v.BlockDate,
                    Blockreason = v.BlockReason,
                    Kinship = v.Kinship,
                    Typeuser = v.Typeuser
                };
                return ben;

            }
        }
    }

    public class FamilyCb
    {
        public Guid Guid { get; set; }
        public Guid Hubguid { get; set; }
        public Guid personguid { get; set; }
        public string aggregator { get; set; }
        public List<BeneficiaryIn> Family { get; set; }

        public class BeneficiaryIn : Beneficiary
        {
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