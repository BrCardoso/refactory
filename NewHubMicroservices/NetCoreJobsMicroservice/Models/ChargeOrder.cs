using System;
using System.Collections.Generic;
using Commons.Enums;

namespace NetCoreJobsMicroservice
{
    public class ChargeOrder
    {
        public Guid Guid { get; set; }
        public string DocType { get; set; }
        public Guid Hubguid { get; set; }
        public Guid Providercustomerguid { get; set; }
        public Guid Rulesconfigurationguid { get; set; }
        public Guid Companyguid { get; set; }
        public string Aggregator { get; set; }
        public string Competence { get; set; }
        public string Subsegcode { get; set; }
        public List<ChargeElement> Charges { get; set; }

        public partial class ChargeElement
        {
            public ChargeElement()
            { }

            public ChargeElement(Guid personguid, string name, string cpf, string card, float value, float total, DateTime registrationdate, RequestreturnLs requestreturn)
            {
                Personguid = personguid;
                Name = name;
                CPF = cpf;
                Card = card;
                Value = value;
                Total = total;
                Registrationdate = registrationdate;
                Requestreturn = requestreturn;
            }

            public Guid Personguid { get; set; }
            public string Name { get; set; }
            public string CPF { get; set; }
            public string Card { get; set; }
            public float Value { get; set; }
            public float Debit { get; set; }
            public float Credit { get; set; }
            //public string Status { get; set; }
            public float Total { get; set; }
            public DateTime Registrationdate { get; set; }
            public DateTime Requestdate { get; set; }
            public RequestreturnLs Requestreturn { get; set; }
        }

        public partial class RequestreturnLs
        {
            public RequestreturnLs()
            { }

            public HubMovementStatus Status { get; set; }
            public string Description { get; set; }
            public string Protocol { get; set; }
            public DateTime Returndate { get; set; }
            public List<string> Inconsistencies { get; set; }
        }

    }
}
