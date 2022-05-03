using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace chargeAppServices.Models
{
    public class Beneficiary : Person
    {
        public string Origin { get; set; }
        public Guid PersonGuid { get; set; }
        public string Sequencial { get; set; }
        public string Kinship { get; set; }
        public string Typeuser { get; set; }
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; }
        public List<Benefitinfo> Benefitinfos { get; set; }
    }

    public class BeneficiaryAddInfo
    {
        public string Origin { get; set; }
        public string PersonGuid { get; set; }
        public string Sequencial { get; set; }
        public string Kinship { get; set; }
        public string Typeuser { get; set; }
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; }
        public List<Benefitinfo> Benefitinfos { get; set; }

    }

    public partial class Benefitinfo : IBenefitinfo
    {
        /// <summary>
        /// usado somente no cliente
        /// </summary>
        public Guid providerguid { get; set; }
        public string providerid { get; set; }
        [NotEmpty]
        public string Productcode { get; set; }
        [NotEmpty]
        public string ContractNumber { get; set; }
        public string Cardnumber { get; set; }
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; }
        /// <summary>
        /// Indica se o beneficiario deve ser enviado para INCLUSÃO no provedor quando realizado a primeira carga de dados
        /// </summary>
        public bool Sync { get; set; }
        public bool Synced { get; set; }
        public List<Complementaryinfo> Complementaryinfos { get; set; }
    }

    public interface IBenefitinfo
    {
        public Guid providerguid { get; set; }
        public string providerid { get; set; }
        public string Productcode { get; set; }
        public string ContractNumber { get; set; }
        public string Cardnumber { get; set; }
        public DateTime? BlockDate { get; set; }
        public string BlockReason { get; set; }
    }
}
