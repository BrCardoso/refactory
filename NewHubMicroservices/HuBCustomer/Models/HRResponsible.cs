using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace HuBCustomerAppService.Models
{
    public class HREmployees
    {
        public DateTime admissiondate { get; set; }

        public string costcenter { get; set; }

        public string costcentercode { get; set; }

        public string department { get; set; }

        public string departmentcode { get; set; }

        public object employeecomplementaryinfos { get; set; }

        public string familyguid { get; set; }

        public object functionalcategory { get; set; }

        public object functionalcategorycode { get; set; }

        public object occupation { get; set; }

        public object occupationcode { get; set; }

        public string personguid { get; set; }

        public string registration { get; set; }

        public string role { get; set; }

        public string rolecode { get; set; }

        public int? salary { get; set; }

        public string shift { get; set; }

        public object union { get; set; }

        public object unioncode { get; set; }
    }

    public class HRResponsibles
    {
        public List<HREmployees> employees { get; set; }

    }
}
