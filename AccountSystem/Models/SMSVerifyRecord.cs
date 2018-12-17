using AccountSystem.Models.SMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Models
{
    public class SMSVerifyRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Sid { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime Deadline { get; set; }
        public VerifyType Type { get; set; }
        public bool Valid { get; set; } = true;
    }
}
