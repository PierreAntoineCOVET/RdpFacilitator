using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpFacilitator
{
    public class RdpLauncher
    {
        private readonly string Arguments;

        private RdpLauncher(string arguments)
        {
            Arguments = arguments;
        }

        public static RdpLauncher FromFile(string fileName) => new RdpLauncher($"\"{fileName}\"");

        public static RdpLauncher FromAddress(string address) => new RdpLauncher($"/v:\"{address}\"");

        public void StartRemoteSession()
        {
            var processStartInfo = new ProcessStartInfo("mstsc", Arguments);

            Process.Start(processStartInfo);
        }
    }
}
