using Commons;
using Commons.Base;
using Commons.Enums;
using System;
using System.Collections.Generic;

namespace BeneficiaryAppService.Models
{

    public class QueueModel

    {

        public Guid guid { get; set; }

        public Guid Hubguid { get; set; }

        public Guid ProviderGuid { get; set; }

        public string ProviderName { get; set; }

        public string Aggregator { get; set; }

        public MovimentTypeEnum MovementType { get; set; }

        public string Status { get; set; }

        public DateTime Incdate { get; set; }

        public LinkInformation LinkInformation { get; set; }

        public List<QueueBeneficiary> Beneficiary { get; set; }

        public Charge Charge { get; set; }

    }

    public class QueueBeneficiary : Person
    {

        public Guid personguid { get; set; }

        public string TypeUser { get; set; }

        public string Kinship { get; set; }

        public Return Return { get; set; }

        public Holder Holder { get; set; }

        public Benefitinfo BenefitInfos { get; set; }

    }

	public class Holder
	{
		public Guid personguid { get; set; }
		public string CardNumber { get; set; }
		public string Cpf { get; set; }
		public string Name { get; set; }
		public EmployeeinfoClean JobInfo { get; set; }
	}

	public class EmployeeinfoClean : IEmployeeinfo
	{
		#region IEmployeeInfo

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

		public static explicit operator EmployeeinfoClean(EmployeeInfo v)
		{
			EmployeeinfoClean ret = new EmployeeinfoClean
			{
				Admissiondate = v.Admissiondate,
				Costcenter = v.Costcenter,
				Costcentercode = v.Costcentercode,
				Department = v.Department,
				Departmentcode = v.Departmentcode,
				Employeecomplementaryinfos = v.Employeecomplementaryinfos,
				Functionalcategory = v.Functionalcategory,
				Functionalcategorycode = v.Functionalcategorycode,
				Occupation = v.Occupation,
				Occupationcode = v.Occupationcode,
				Registration = v.Registration,
				Role = v.Role,
				Rolecode = v.Rolecode,
				Salary = v.Salary,
				Shift = v.Shift,
				Union = v.Union,
				Unioncode = v.Unioncode
			};
			return ret;
		}

		#endregion IEmployeeInfo
	}

	public partial class Return
	{
		public string Status { get; set; }
		public string Response { get; set; }
		public DateTime Datetime { get; set; }
	}

	public partial class Charge
	{
		public string subsegcode { get; set; }
		public List<ChargeBeneficiary> Beneficiary { get; set; }
		public VrAddicionalInfo VrAddicionalInfo { get; set; }
	}

	public class ChargeBeneficiary
	{
		public Return Return { get; set; }
		public string Sequential { get; set; }
		public Guid personguid { get; set; }
		public string IdOnProvider { get; set; }
		public string Name { get; set; }
		public string Cpf { get; set; }
		public DateTime BirthDate { get; set; }
		public string Gender { get; set; }
		public List<Emailinfo> Emailinfos { get; set; }
		public Chargeinfo Chargeinfo { get; set; }
	}

	public class Chargeinfo
	{
		public string Product { get; set; }
		public DateTime ChargeDate { get; set; }
		public float CreditValue { get; set; }
	}

	public class VrAddicionalInfo
	{
		public string CToken { get; set; }
		public string Identificador { get; set; }
		public string Usuario { get; set; }
		public string SiglaEmissor { get; set; }
		public string SiglaCanal { get; set; }
		public string CnpjRh { get; set; }
		public string HashChaveEmissor { get; set; }
	}

	public class LinkInformation
	{
		public string CostumerNumber { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string ContractIssued { get; set; }
		public string ContractNumber { get; set; }
		public string ResponsiblePerson { get; set; }
		public string ResponsibleId { get; set; }
		public string ResponsibleBirthDate { get; set; }
	}
}