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
        private readonly string FileName;

        public RdpLauncher(string fileName)
        {
            FileName = fileName;
        }

        public void StartRemoteSession()
        {
            var processStartInfo = new ProcessStartInfo("mstsc", FileName);

            Process.Start(processStartInfo);
        }
    }
}
