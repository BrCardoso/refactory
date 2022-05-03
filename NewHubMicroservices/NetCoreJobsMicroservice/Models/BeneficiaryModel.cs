using Commons;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public partial class BeneficiariesModel :MethodFeedback
    {
        public List<Beneficiary> Beneficiary { get; set; }

        internal Dictionary<string, object> Validate()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();


            return dict;
        }
    }

    public partial class Beneficiary : PersonModel
    {

        public string Typeuser { get; set; }
        public string Origin { get; set; }
        public string Kindship { get; set; }
        public List<Benefit> Benefits { get; set; }
        public Ownership Ownership { get; set; }
        public Employeeinfo Employeeinfo { get; set; }
    }
    
    public partial class Benefit
    {
        public string Segcode { get; set; }

        public string Provider { get; set; }

        public string Providercode { get; set; }

        public string Product { get; set; }

        public string Productcode { get; set; }

        public string Cardnumber { get; set; }

        public string Startdate { get; set; }

        public string Blockdate { get; set; }

        public string Blockmotive { get; set; }

        public Tranference Tranference { get; set; }
    }
    public partial class Tranference
    {
        public string Providercode { get; set; }

        public string Productcode { get; set; }

        public string Effectivedate { get; set; }
    }
    public partial class Employeeinfo
    {
        public string Registration { get; set; }

        public string Originbranch { get; set; }

        public string Cpts { get; set; }

        public string Pispasep { get; set; }

        public string Admissiondate { get; set; }

        public string Demissiondate { get; set; }

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
    }
    public partial class Ownership
    {
        public string Originbranch { get; set; }

        public string Registration { get; set; }

        public string Sequencial { get; set; }
    }
}