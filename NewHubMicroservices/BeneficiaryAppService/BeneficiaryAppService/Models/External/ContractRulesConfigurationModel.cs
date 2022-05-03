using Commons;
using System;
using System.Collections.Generic;

namespace BeneficiaryAppService.Models.External
{
    #region RCbase
    public class RulesConfigurationModel
    {
        public Guid guid { get; set; }
        [NotEmpty]
        public Guid hubguid { get; set; }
        [NotEmpty]
        public Guid providerguid { get; set; }
        [NotEmpty]
        public string aggregator { get; set; }
        [NotEmpty]
        public string segcode { get; set; }
        public string contractrulename { get; set; }
        public string description { get; set; }
        [NotEmpty]
        public string contractnumber { get; set; }
        //cnpj informado no contrato com o provedor
        [NotEmpty]
        public string contractIssued { get; set; }
        public string duedate { get; set; }
        public System.DateTime effectivedate { get; set; }
        public hrresponsable hrresponsable { get; set; }
        public List<Products> products { get; set; }
        public List<healthrules> healthrules { get; set; }
        public List<productruleHealth> generalproductruleshealth { get; set; }

        public List<Foodrule> foodrules { get; set; }
        public List<productruleFood> generalproductrulesFood { get; set; }
    }

    public class productruleHealth
    {
        [NotEmpty]
        public string productrulename { get; set; }
        [NotEmpty]
        public string elegibilitylimitations { get; set; }
        [ListCannotBeEmpty]
        public List<item> items { get; set; }
    }

    public class productruleFood
    {
        [NotEmpty]
        public string type { get; set; }
        [NotEmpty]
        public string productrulename { get; set; }
        [NotEmpty]
        public string elegibilitylimitations { get; set; }
        [ListCannotBeEmpty]
        public List<item> items { get; set; }
        public float benefitValue { get; set; }
        public float? valueForExtraHour { get; set; }
    }

    public class Products
    {
        public string code { get; set; }
        public List<string> productrulenames { get; set; }

        [NotEmpty]
        public string productpricetablename { get; set; }
    }

    public class healthrules
    {
        public limitdates limitdates { get; set; }

        [ListCannotBeEmpty]
        public List<kinship> kinship { get; set; }

        //periodo em meses do funcionario em experiencia antes de ter direito ao beneficio
        public int employeeontry { get; set; }
        public float cardduplicated { get; set; }
        public string enableapp { get; set; }

        //mes especifico para movimentação no contrato
        public int? specificmonth { get; set; }
        public int minimumperiod { get; set; }
    }

    public class Foodrule
    {
        public string Companyname { get; set; }
        public int Employeeentry { get; set; }
        public int Daymonthtocharge { get; set; }
        public int AlertHowManyDaysBefore { get; set; }
        public string Cardduplicated { get; set; }
        public bool Calculatedbyhub { get; set; }
        public string FixedOrWorkingDays { get; set; }
        public int QuantityOfFixedDays { get; set; }
        public Workingdays Workingdays { get; set; }
        public bool? DiscountVacationsOrAbsences { get; set; }
        public bool? DiscountsFouls { get; set; }
        public bool? DiscountsHolidays { get; set; }
        public bool? AdditionalForExtraHour { get; set; }
    }

    public class hrresponsable
    {
        [NotEmpty]
        public string name { get; set; }
        [NotEmpty]
        public string CPF { get; set; }
        [NotEmpty]
        public string birthdate { get; set; }
        public List<Phoneinfo> phoneinfo { get; set; }
        public List<ContactInfo> emailInfo { get; set; }
    }

    public class date
    {
        public string day { get; set; }
        public string refdate { get; set; }
    }

    public class limitdates
    {
        [NotEmpty]
        public string type { get; set; }
        public List<date> dates { get; set; }
    }

    public class kinship
    {
        [NotEmpty]
        public string value { get; set; }
        public int? ageLimit { get; set; }
        public int? studentAgeLimit { get; set; }
        public bool? deletekinship { get; set; }
    }


    public class Workingdays
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }

    public class item
    {
        [NotEmpty]
        public string code { get; set; }
        public string description { get; set; }
    }
    public partial class ContactInfo
    {
        public string Type { get; set; }

        public string Email { get; set; }

        public bool? IsDefault { get; set; }
    }

    #endregion
}
