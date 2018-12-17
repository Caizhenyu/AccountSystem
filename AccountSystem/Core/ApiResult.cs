using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Core
{
    public class ApiResult
    {
        public object data { get; set; }
        public int error_Code { get; set; }
        public string msg { get; set; }
        public string request { get; set; }
    }
}
