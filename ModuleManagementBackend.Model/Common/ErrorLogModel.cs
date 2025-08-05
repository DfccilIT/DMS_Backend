using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.Common
{
    public class ErrorLogModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string RequestPath { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
