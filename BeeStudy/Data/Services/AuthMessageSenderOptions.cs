using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Data.Services
{
    public class AuthMessageSenderOptions
    {
        public string SendGridKey { get; set; }

        public string UdemyClientId { get; set; }

        public string UdemyClientSecret { get; set; }
    }
}
