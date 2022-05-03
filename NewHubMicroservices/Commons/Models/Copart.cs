using System;

namespace Commons.Base
{
    public class Copart
    {
        public string Registration { get; set; }
        public DateTime Eventdate { get; set; }
        //CNPJ do provedor
        public string Providercnpj { get; set; }
        public string Cardnumber { get; set; }
        public float Employeevalue { get; set; }
        public float Companyvalue { get; set; }
        public string Refdate { get; set; }
        public string Servicesuppliercnpj { get; set; }
        public string Servicesuppliername { get; set; }
        public string Medicalspecialtie { get; set; }
        public string Cid { get; set; }
        public string Servicetype { get; set; }
        public string Description { get; set; }
        private string Beneficiaryname;
        public string beneficiaryname
        {
            get { return Beneficiaryname; }
            set { Beneficiaryname = value?.Trim(); }
        }
        public int Amount { get; set; }
    }
}
