using System;
using System.Collections.Generic;

namespace Commons.Base
{
    public class CompanyStruc : Company
    {
        public List<Employee> Employees { get; set; }
        public List<ProviderStruc> Providers { get; set; }
        public List<Entity> Entities { get; set; }
        public List<Copart> Copart { get; set; }
        public List<Insuranceclaim> Insuranceclaim { get; set; }

        //public CompanyStruc()
        //{
        //    this.Employees = new List<Employee> { new Employee()};
        //    this.Providers = new List<ProviderStruc> { new ProviderStruc()};
        //    this.Entities = new List<Entity> { new Entity()};
        //    this.Copart = new List<Copart> { new Base.Copart()};
        //    this.Insuranceclaim = new List<Insuranceclaim> { new Base.Insuranceclaim()};
        //}
    }
    
}
