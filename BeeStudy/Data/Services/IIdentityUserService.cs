using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public interface IIdentityUserService
    {
        IEnumerable<IdentityUser> GetAll();

        IdentityUser GetById(string id);

        IdentityUser GetByEmail(string email);

        void Delete(string id);


    }
}
