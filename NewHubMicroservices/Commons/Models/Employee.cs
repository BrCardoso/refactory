using System;
using System.Collections.Generic;


namespace Commons.Base
{
    public class Employee : Employeeinfo
    {
        public List<Beneficiary> Family { get; set; }
        //public Employee()
        //{
        //    this.Family = new List<Beneficiary> { new Beneficiary() };
        //}
    }

    public class Employeeinfo : Person, IEmployeeinfo
    {
        public string Registration { get; set; }

        private DateTime _Admissiondate;
        public DateTime Admissiondate
        {
            get { return _Admissiondate; }
            set { _Admissiondate = value.Date; }
        }

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
        
    }

    public interface IEmployeeinfo
    {
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

        //public Employeeinfo() {
        //    this.Employeecomplementaryinfos = new List<Complementaryinfo> { new Complementaryinfo()};
        //}
    }

}
