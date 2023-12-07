using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpFacilitator
{
    public class MonitorFormatException : Exception
    {
        public MonitorFormatException() : base() { }

        public MonitorFormatException(string error) : base(error) { }
    }
}
