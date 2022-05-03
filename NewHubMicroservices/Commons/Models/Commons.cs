using System;

namespace Commons
{
    public class MethodFeedback
    {
        public bool Success { get; set; }

        public bool Exception { get; set; }

        public string Message { get; set; }

        public string MessageCode { get; set; }
        public int HttpStatusCode { get; set; }

        public object obj { get; set; }

        public MethodFeedback()
        {
            this.Success = true;
            this.Exception = false;
        }
    }
    public class Address
    {
        private string? _type;

        public string? type
        {
            get { return _type; }
            set { _type = string.IsNullOrEmpty(value) ? "RESIDENCIAL" : value; }
        }
        public string zipcode { get; set; }
        public string patiotype { get; set; }
        public string street { get; set; }
        public int number { get; set; }
        public string addicionalinfo { get; set; }
        public string neighborhood { get; set; }
        public string city { get; set; }
        public string state { get; set; }

        private string? _country;
        public string? country
        {
            get { return _country; }
            set { _country = string.IsNullOrEmpty(value) ? "BRASIL" : value; }
        }

        public bool? isdefault { get; set; }
    }
    public class Phoneinfo
    {
        public string type { get; set; }
        public string countrycode { get; set; }
        public string area { get; set; }
        public string phonenumber { get; set; }
        public string extension { get; set; }
        public bool? isdefault { get; set; }
    }
    public class Emailinfo
    {
        public string type { get; set; }
        public string email { get; set; }
        public bool? isdefault { get; set; }
    }
    public class Complementaryinfo
    {
        public string type { get; set; }
        public string value { get; set; }
    }
    public class Document
    {
        public string type { get; set; }
        public DateTime incdate { get; set; }
        public string image_front { get; set; }
        public string image_back { get; set; }
        public string complementaryinfo { get; set; }
    }
    public class Change
    {
        public DateTime Date { get; set; }

        public string Attribut { get; set; }

        public object Oldvalue { get; set; }

        public object Newvalue { get; set; }

        public bool Sync { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}
