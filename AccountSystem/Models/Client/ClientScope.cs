using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Models.Client
{
    public class ClientScope
    {
        [Required]
        public string Client { get; set; }
        public string Scope { get; set; }
    }
}
