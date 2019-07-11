using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.ViewModels
{
    //view model for changeroles screen
    public class ChangeRoles
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool IsTodoRegistered { get; set; }
        public bool IsMembershipRegistered { get; set; }
        public bool IsRoleRegistered { get; set; }
    }
}
