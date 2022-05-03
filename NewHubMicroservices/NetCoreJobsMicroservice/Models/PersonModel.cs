using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class PersonModel
    {
        public Guid GUID { get; set; }
        public string Name { get; set; }

        public string Gender { get; set; }

        public string Cpf { get; set; }

        public string Birthdate { get; set; }

        public string Rg { get; set; }

        public string Issuingauthority { get; set; }

        public string Cns { get; set; }

        public string Dnv { get; set; }

        public string Mothername { get; set; }

        public string Maritalstatus { get; set; }

        public string Weddingdate { get; set; }

        public bool? Pwd { get; set; }

        public bool? Collegestudant { get; set; }

        public string Weight { get; set; }

        public string Height { get; set; }

        public List<Address> Address { get; set; }

        public List<Phoneinfo> Phoneinfo { get; set; }
        public string Email { get; set; }
    }

    public partial class Address
    {
        public string Type { get; set; }

        public string Zipcode { get; set; }

        public string Patiotype { get; set; }

        public string Street { get; set; }

        public string Number { get; set; }

        public string Addicionalinfo { get; set; }

        public string Neighborhood { get; set; }

        public string City { get; set; }

        public string State { get; set; }
        public string Country { get; set; }
        public bool? IsDefault { get; set; }
    }
    
    public partial class Phoneinfo
    {
        public string Type { get; set; }

        public string Countrycode { get; set; }

        public string Area { get; set; }

        public string Phonenumber { get; set; }

        public string Extension { get; set; }
        public bool? IsDefault { get; set; }

    }

    public partial class ContactInfo
    {
        public string Type { get; set; }

        public string Email { get; set; }

        public bool? IsDefault { get; set; }
    }
}
