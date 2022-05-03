using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class EntityOut
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public string aggregator { get; set; }
        public List<Entity> entities { get; set; }

        public static explicit operator EntityOut(List<Entity> v)
        {
            EntityOut entityOut = new EntityOut { 
                entities = v
            };

            return entityOut;
        }
    }
}
