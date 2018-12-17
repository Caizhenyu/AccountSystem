using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Models.Client
{
    public class ClientLifetime
    {
        [Required]
        public string Client { get; set; }
        [Range(1,Int32.MaxValue)]
        public int Lifetime { get; set; }
    }
}
