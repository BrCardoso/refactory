using Commons;
using Commons.Base;
using System.Collections.Generic;

namespace BeneficiaryAppService.Models
{
    public class ValidateByContractModel
    {
        public string ErrorKey { get; set; }
        public object ErrorValue { get; set; }
        public List<invalidatedPeople> InvalidatedPeople { get; set; }
        public class invalidatedPeople
        {
            public string Name { get; set; }
            public string CPF { get; set; }
            public string Kinship { get; set; }
            public string Employeename { get; set; }
            public string EmployeeRegistration { get; set; }
        }

        public ValidateByContractModel() { InvalidatedPeople = new List<invalidatedPeople>(); }
    }
}
