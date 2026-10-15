using CommandLine;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace RdpFacilitator
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandLineOption = Parser.Default.ParseArguments<CommandLineOption>(args)
                .WithParsed(DoWork)
                .WithNotParsed(DisplayError);
        }

        static void DoWork(CommandLineOption options)
        {
            // V2 Clean password
            // from https://superuser.com/questions/1756354/windows-defender-credential-guard-does-not-allow-using-saved-credentials-for-r
            // List cmdkey /list:TERMSRV/*
            // cmdkey /delete:TERMSRV/<targetNameOrIp>

            if (options.RowTolerance < 0)
            {
                Console.WriteLine("Option -t (rowTolerance) must be 0 or more pixels.");
                return;
            }

            var displayInfos = HardwareDetector.GetDisplayInfos(options.Verbose);

            var monitorManager = new MonitorManager(displayInfos, options.SelectedMonitors ?? [], options.Verbose, options.RowTolerance);
            if (options.DisplayMonitorGrid)
            {
                monitorManager.PrintDetectedMonitorGrid();
                return;
            }

            if (options.SelectedMonitors == null || !options.SelectedMonitors.Any() || string.IsNullOrWhiteSpace(options.RdpFilePath))
            {
                Console.WriteLine("Options -m (monitors) and -f (rdpFile) are required unless -l is used.");
                Console.WriteLine("Use -h --help for detail about parameters format.");
                return;
            }

            var selectedMonitors = monitorManager.GetSelectMonitors();

            if (selectedMonitors == null)
            {
                return;
            }

            if(options.Verbose)
            {
                Console.WriteLine($"Selected monitors : {selectedMonitors}");
            }

            var fileUpdater = new FileUpdater(options.RdpFilePath, options.SaveMode, selectedMonitors, options.Address);

            string rdpFileName;
            try
            {
                rdpFileName = fileUpdater.UpdateRdpFile();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (!options.NoLaunch)
            {
                var rdpLauncher = new RdpLauncher(rdpFileName);
                rdpLauncher.StartRemoteSession();
            }
        }

        static void DisplayError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
