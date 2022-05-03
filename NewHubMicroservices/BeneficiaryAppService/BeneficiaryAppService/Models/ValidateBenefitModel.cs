using Commons;
using Commons.Base;

namespace BeneficiaryAppService.Models
{
    public class ValidateBenefitModel
    {
        [NotEmpty]
        public string kinship { get; set; }
        [NotEmpty]
        public string typeuser { get;  set; }
        [NotEmpty]
        public PersonDB Person { get; set; }
        [NotEmpty]
        public Benefitinfo Benefitinfo { get; set; }
        [NotEmpty]
        public EmployeeInfo EmployeeInfo { get; set; }
    }
}
