using Commons;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace chargeAppServices.Models
{
    public class EmployeesModel
    {
        public Guid guid { get; set; }
        [NotEmpty]
        public Guid hubguid { get; set; }
        [NotEmpty]
        public string aggregator { get; set; }
        public List<EmployeeInfo> employees { get; set; }
    }

    public class EmployeeInfo
    {
        public Guid personguid { get; set; }
        public Guid familyguid { get; set; }
        #region IEmployeeInfo
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
        #endregion
    }

}
