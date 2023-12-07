using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpFacilitator
{
    public class MonitorManager
    {
        private readonly IEnumerable<DetectedMonitor> DetectedMonitorInfos;
        private readonly IEnumerable<string> SelectedMonitorsString;
        private readonly bool Verbose;
        private readonly List<RequestedMonitorPosition> SelectedMonitorPositions = new List<RequestedMonitorPosition>();

        public MonitorManager(IEnumerable<DetectedMonitor> detectedMonitors, IEnumerable<string> selectedMonitors, bool verbose)
        {
            DetectedMonitorInfos = detectedMonitors;
            SelectedMonitorsString = selectedMonitors;
            Verbose = verbose;
        }

        public void PrintDetectedMonitorGrid()
        {
            ComputeDetectedMonitorGridIndex();

            foreach (var displayInfo in DetectedMonitorInfos)
            {
                Console.WriteLine($"RowIndex        : {displayInfo.RowIndex}");
                Console.WriteLine($"ColumnIndex     : {displayInfo.ColumnIndex}");
                Console.WriteLine($"DeviceName      : {displayInfo.DeviceName}");
                Console.WriteLine($"DeviceNumber    : {displayInfo.DeviceNumber}");
                Console.WriteLine($"TopMost         : {displayInfo.TopMost}");
                Console.WriteLine($"LeftMostPosition: {displayInfo.LeftMostPosition}");
                Console.WriteLine();
            }
        }

        private void ComputeDetectedMonitorGridIndex()
        {
            var rows = DetectedMonitorInfos
                .Select(d => d.TopMost)
                .Distinct()
                .Order()
                .ToList();

            int rowIndex = 0, columnIndex = 0;

            foreach (var row in rows)
            {
                var monitorsOnRow = DetectedMonitorInfos
                    .Where(d => d.TopMost == row)
                    .ToList();

                var columns = monitorsOnRow
                    .Select(d => d.LeftMostPosition)
                    .Distinct()
                    .Order()
                    .ToList();

                foreach (var colum in columns)
                {
                    var monitors = monitorsOnRow
                        .Where(d => d.LeftMostPosition == colum)
                        .ToList();

                    if (monitors.Count != 1)
                    {
                        throw new Exception($"{monitors.Count} monitor(s) detected insted of 1.");
                    }

                    monitors[0].RowIndex = rowIndex;
                    monitors[0].ColumnIndex = columnIndex;

                    columnIndex++;
                }

                columnIndex = 0;
                rowIndex++;
            }
        }

        public string GetSelectMonitors()
        {
            if(!ParseSelectedMonitors())
            {
                Console.WriteLine("Use -h --help for detail about parameters format.");
                PrintDetectedMonitorGrid();
            }

            ComputeDetectedMonitorGridIndex();

            var selectedMonitorsNumbersForRdp = new StringBuilder();

            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
            }

            foreach (var selectedMonitorPosition in SelectedMonitorPositions)
            {
                var matchingDetectedMonitor = DetectedMonitorInfos
                    .Where(d => d.RowIndex == selectedMonitorPosition.RowIndex && d.ColumnIndex == selectedMonitorPosition.ColumnIndex)
                    .SingleOrDefault();

                if(matchingDetectedMonitor != null && !string.IsNullOrWhiteSpace(matchingDetectedMonitor.DeviceNumber))
                {
                    if (Verbose)
                    {
                        Console.Write($"Matching device {matchingDetectedMonitor.DeviceName} with number {matchingDetectedMonitor.DeviceNumber}");
                        Console.WriteLine($" at position {selectedMonitorPosition.RowIndex}:{selectedMonitorPosition.ColumnIndex}");
                    }

                    if(selectedMonitorsNumbersForRdp.Length > 0)
                    {
                        selectedMonitorsNumbersForRdp.Append(",");
                    }

                    selectedMonitorsNumbersForRdp.Append(int.Parse(matchingDetectedMonitor.DeviceNumber));
                }
            }

            return selectedMonitorsNumbersForRdp.ToString();
        }

        private bool ParseSelectedMonitors()
        {
            try
            {
                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                }

                SelectedMonitorPositions.AddRange(SelectedMonitorsString
                    .Select(ParseSelectedMonitor));

                return true;
            }
            catch (MonitorFormatException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private RequestedMonitorPosition ParseSelectedMonitor(string selectedMonitor)
        {
            var monitorPosition = selectedMonitor.Split(":");

            if (monitorPosition.Length != 2)
                throw new MonitorFormatException("Invalid monitors parameter format: need to have both row and column indexes.");

            if(!int.TryParse(monitorPosition[0], out var rowIndex)
                || !int.TryParse(monitorPosition[1], out var columnIndex))
            {
                throw new MonitorFormatException("Invalid monitors parameter format: row and column indexes must be valid integer, 0 based.");
            }

            if (Verbose)
            {
                Console.WriteLine($"Requested monitor {rowIndex}:{columnIndex}");
            }

            return new RequestedMonitorPosition
            {
                RowIndex = rowIndex,
                ColumnIndex = columnIndex
            };
        }
    }
}
