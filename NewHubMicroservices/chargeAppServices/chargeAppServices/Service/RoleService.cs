using ChargeAppServices.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargeAppServices.Service
{
    public class RoleService : IElegibilityService
    {
        public bool Verify()
        {
            return true;
        }
    }
}
