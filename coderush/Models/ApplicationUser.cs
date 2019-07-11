using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        //override identity user, add new column
        public bool isSuperAdmin { get; set; } = false;
        
    }
}
