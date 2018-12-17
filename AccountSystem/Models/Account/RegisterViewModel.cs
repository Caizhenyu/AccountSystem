using AccountSystem.Models.SMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Models.Account
{
    public class RegisterViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }        
        public string Role { get; set; }
        public string VerifyCode { get; set; }
        public VerifyType Type { get; set; }
    }
}
