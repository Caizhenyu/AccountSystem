using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Models.Account
{    
    public class LoginViewModel
    {
        public string Authority { get; set; } 
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
    }
}
