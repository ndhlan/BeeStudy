using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Models
{
    public class RegisteredUserViewModel
    {
        public IdentityUser User { get; set; }

        public Learner Learner { get; set; }
    }
}
