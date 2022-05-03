using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;

namespace JobsMicroservice.Model
{
    #region Entrada
    public class QueueModel
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string aggregator { get; set; }
        public string movementtype { get; set; }
        public string status { get; set; }
        public DateTime incdate { get; set; }
        public LinkInformation linkinformation { get; set; }
        public List<QueueBeneficiary> beneficiary { get; set; }
        public charge charge { get; set; }
    }

    public class QueueBeneficiary : Person
    {
        public Guid personguid { get; set; }
        public string typeuser { get; set; }
        public string kinship { get; set; }
        public Return Return { get; set; }
        public Holder holder { get; set; }
        public BenefitInfos benefitinfos { get; set; }
    }

    public class BenefitInfos
    {
        public string providerproductcode { get; set; }
        public string product { get; set; }
        public string subsegment { get; set; }
        public string cardnumber { get; set; }
        public DateTime? startdate { get; set; }
        public DateTime? blockdate { get; set; }
        public string blockreason { get; set; }
        public Transference transference { get; set; }
        public string reissuereason { get; set; }
    }

    public class Transference
    {
        public string providerproductcode { get; set; }
        public string product { get; set; }
        public DateTime startdate { get; set; }
    }

    public class Holder
    {
        public Guid personguid { get; set; }
        public string cardnumber { get; set; }
        public string cpf { get; set; }
        public string name { get; set; }
        public EmployeeinfoClean jobinfo { get; set; }
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
        #endregion
    }

    public partial class Return
    {
        public string status { get; set; }
        public string response { get; set; }
        public DateTime datetime { get; set; }
    }

    public partial class charge
    {
        public string subsegcode { get; set; }
        public List<ChargeBeneficiary> beneficiary { get; set; }
        public VrAddicionalInfo vraddicionalinfo { get; set; }
    }

    public class ChargeBeneficiary
    {
        public Return Return { get; set; }
        public string sequential { get; set; }
        public Guid personguid { get; set; }
        public string idonprovider { get; set; }
        public string name { get; set; }
        public string cpf { get; set; }
        public DateTime birthdate { get; set; }
        public string gender { get; set; }
        public List<Emailinfo> emailinfos { get; set; }
        public Chargeinfo chargeinfo { get; set; }
    }

    public class Chargeinfo
    {
        public string product { get; set; }
        public DateTime chargedate { get; set; }
        public float creditvalue { get; set; }
    }

    public class VrAddicionalInfo
    {
        public string ctoken { get; set; }
        public string identificador { get; set; }
        public string usuario { get; set; }
        public string siglaemissor { get; set; }
        public string siglacanal { get; set; }
        public string cnpjrh { get; set; }
        public string hashchaveemissor { get; set; }
    }

    public class LinkInformation
    {
        public string costumernumber { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string contractissued { get; set; }
        public string contractnumber { get; set; }
        public string responsibleperson { get; set; }
        public string responsibleid { get; set; }
        public DateTime? responsiblebirthdate { get; set; }
    }
    #endregion

    #region EntradaResponse

    public class ResponseQueue : Return
    {
        public Guid personguid { get; set; }
    }
    public class ResponseNIT
    {
        public Guid movementguid { get; set; }
        public List<ResponseQueue> Return { get; set; }
    }
    #endregion

    #region SaidaNIT
    public partial class NitModel
    {
        public Customer customer { get; set; }
        public Provider provider { get; set; }
        public string movementtype { get; set; }
        public Guid movementguid { get; set; }
        public LinkInformation linkinformation { get; set; }
        public List<QueueBeneficiary> beneficiary { get; set; }
        public charge charge { get; set; }
    }

    public partial class Customer
    {
        public string aggregator { get; set; }
        public string group { get; set; }
        public string companyname { get; set; }
        public string branchname { get; set; }
        public string companyid { get; set; }
        public List<Commons.Address> address { get; set; }
    }

    public partial class Provider
    {
        public Guid providerguid { get; set; }
        public string name { get; set; }
        public string site { get; set; }
        public string cnpj { get; set; }
        public string segment { get; set; }
        public string providerregistrationcode { get; set; }
        public string email { get; set; }
    }

    #endregion

    #region SaidaNITResponse
    public class NITResponse
    {
        public string Message { get; set; }
        public NitModel Data { get; set; }
    }
    #endregion
}