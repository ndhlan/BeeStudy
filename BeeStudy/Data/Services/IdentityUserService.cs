using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly ApplicationDbContext _context;

        public IdentityUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Delete(string id)
        {
            var userToDelete = _context.Users.FirstOrDefault(u => u.Id.Equals(id));
            _context.Users.Remove(userToDelete);
            _context.SaveChanges();

        }

        public IEnumerable<IdentityUser> GetAll()
        {
            var result = _context.Users.ToList();

            return result;
        }

        public IdentityUser GetByEmail(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.Equals(email));
            return user;
        }

        public IdentityUser GetById(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id.Equals(id));
            return user;

        }

    }
}
