using System;
using System.Collections.Generic;

namespace chargeAppServices.Models
{
    public class ChargeOrder
    {
        public Guid Guid { get; set; }
        public string DocType { get; set; }
        public Guid Hubguid { get; set; }
        public Guid Providercustomerguid { get; set; }
        public Guid Rulesconfigurationguid { get; set; }
        public string Aggregator { get; set; }
        public string Competence { get; set; }
        public string Subsegcode { get; set; }
        public List<ChargeElement> Charges { get; set; }

        public partial class ChargeElement
        {            
            public ChargeElement(Guid personguid, string name, string cpf, string card, double value, double total, DateTime registrationdate, RequestreturnLs requestreturn)
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
            public double Value { get; set; }
            public double Debit { get; set; }
            public double Credit { get; set; }
            public string Status { get; set; }
            public double Total { get; set; }
            public DateTime Registrationdate { get; set; }
            public DateTime Requestdate { get; set; }
            public RequestreturnLs Requestreturn { get; set; }
        }

        public partial class RequestreturnLs
        {
            public RequestreturnLs()
            { }

            public Commons.Enums.HubMovementStatus Status { get; set; }
            public string Description { get; set; }
            public string Protocol { get; set; }
            public DateTimeOffset Returndate { get; set; }
            public List<string> Inconsistencies { get; set; }
        }

    }
}
