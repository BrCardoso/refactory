using System;
using System.Collections.Generic;

namespace HUBService.Domain.Models
{
    public class Person
    {
        public string Name { get; set; }

        public string Cpf { get; set; }


        private DateTime _Birthdate;
        public DateTime Birthdate
        {
            get { return _Birthdate; }
            set { _Birthdate = value.Date; }
        }

        public string Gender { get; set; }

        public string Maritalstatus { get; set; }

        public string Mothername { get; set; }

        public string Rg { get; set; }
        public DateTime? expeditiondate { get; set; }

        public string Issuingauthority { get; set; }

        public List<Complementaryinfo> complementaryinfos { get; set; }

        public List<Document> documents { get; set; }

        public List<Address> Addresses { get; set; }

        public List<Phoneinfo> Phoneinfos { get; set; }

        public List<Emailinfo> emailinfos { get; set; }

        public List<Change> Changes { get; set; }

        public Person()
        {
            complementaryinfos = new List<Complementaryinfo>();
        }
    }
}