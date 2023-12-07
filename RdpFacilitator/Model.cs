using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpFacilitator
{
    public class DetectedMonitor
    {
        public required string DeviceName { get; set; }
        public string? DeviceNumber { get; set; }
        public int LeftMostPosition { get; set; }
        public int TopMost { get; set; }
        public int? RowIndex { get; set; }
        public int? ColumnIndex { get; set; }
    }

    public class RequestedMonitorPosition
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
    }
}
