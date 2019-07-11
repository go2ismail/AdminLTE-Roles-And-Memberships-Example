using coderush.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Services.Database
{
    //special service provided for db initialization / data seed
    public class Common : ICommon
    {
        private readonly ApplicationDbContext _context;
        private readonly Security.ICommon _security;

        public Common(ApplicationDbContext context, Security.ICommon security)
        {
            _context = context;
            _security = security;
        }

        public async Task Initialize()
        {
            try
            {
                _context.Database.EnsureCreated();

                //check for users
                if (_context.ApplicationUser.Any())
                {
                    return; //if user is not empty, DB has been seed
                }

                //init app with super admin user
                await _security.CreateDefaultSuperAdmin();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
