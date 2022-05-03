using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Base.RC
{
    #region RCbase

    public interface IRulesConfigurationModel
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string aggregator { get; set; }
        public string segcode { get; set; }
        public string contractrulename { get; set; }
        public string description { get; set; }
        public string contractnumber { get; set; }
        //cnpj informado no contrato com o provedor
        public string contractIssued { get; set; }
        public string duedate { get; set; }
        public System.DateTime effectivedate { get; set; }
        public IHrresponsable hrresponsable { get; set; }
        public List<IProducts> products { get; set; }
        public List<IHealthrules> healthrules { get; set; }
        public List<IProductruleHealth> generalproductruleshealth { get; set; }

        public List<IFoodrule> foodrules { get; set; }
        public List<IProductruleFood> generalproductrulesFood { get; set; }
    }


    public interface IProductruleHealth
    {
        [NotEmpty]
        public string productrulename { get; set; }
        [NotEmpty]
        public string elegibilitylimitations { get; set; }
        [ListCannotBeEmpty]
        public List<IItem> items { get; set; }
    }

    public interface IProductruleFood
    {
        [NotEmpty]
        public string type { get; set; }
        [NotEmpty]
        public string productrulename { get; set; }
        [NotEmpty]
        public string elegibilitylimitations { get; set; }
        [ListCannotBeEmpty]
        public List<IItem> items { get; set; }
        public float benefitValue { get; set; }
        public float? valueForExtraHour { get; set; }
    }

    public interface IProducts
    {
        public string code { get; set; }
        public List<string> productrulenames { get; set; }

        [NotEmpty]
        public string productpricetablename { get; set; }
    }

    public interface IHealthrules
    {
        public ILimitdates limitdates { get; set; }

        [ListCannotBeEmpty]
        public List<IKinship> kinship { get; set; }

        //periodo em meses do funcionario em experiencia antes de ter direito ao beneficio
        public int employeeontry { get; set; }
        public string cardduplicated { get; set; }
        public string enableapp { get; set; }

        //mes especifico para movimentação no contrato
        public int? specificmonth { get; set; }
        public int minimumperiod { get; set; }
    }

    public interface IFoodrule
    {
        public string Companyname { get; set; }
        public int Employeeentry { get; set; }
        public int Daymonthtocharge { get; set; }
        public int AlertHowManyDaysBefore { get; set; }
        public string Cardduplicated { get; set; }
        public bool Calculatedbyhub { get; set; }
        public string FixedOrWorkingDays { get; set; }
        public int QuantityOfFixedDays { get; set; }
        public IWorkingdays Workingdays { get; set; }
        public bool? DiscountVacationsOrAbsences { get; set; }
        public bool? DiscountsFouls { get; set; }
        public bool? DiscountsHolidays { get; set; }
        public bool? AdditionalForExtraHour { get; set; }
    }

    public interface IHrresponsable
    {
        [NotEmpty]
        public string name { get; set; }
        [NotEmpty]
        public string CPF { get; set; }
        [NotEmpty]
        public string birthdate { get; set; }
        public List<Phoneinfo> phoneinfo { get; set; }
        public List<IContactInfo> emailInfo { get; set; }
    }

    public interface IDate
    {
        public string day { get; set; }
        public string refdate { get; set; }
    }

    public interface ILimitdates
    {
        [NotEmpty]
        public string type { get; set; }
        public List<IDate> dates { get; set; }
    }

    public interface IKinship
    {
        [NotEmpty]
        public string value { get; set; }
        public int? ageLimit { get; set; }
        public int? studentAgeLimit { get; set; }
        public bool? deletekinship { get; set; }
    }


    public interface IWorkingdays
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }

    public interface IItem
    {
        [NotEmpty]
        public string code { get; set; }
        public string description { get; set; }
    }
    public interface IContactInfo
    {
        public string Type { get; set; }

        public string Email { get; set; }

        public bool? IsDefault { get; set; }
    }
    #endregion
}
