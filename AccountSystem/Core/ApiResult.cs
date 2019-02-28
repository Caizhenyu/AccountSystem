using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Core
{
    public class ApiResult
    {
        public object Data { get; set; }
        public int Error_Code { get; set; }
        public string Msg { get; set; }
        public string Request { get; set; }
    }
}
