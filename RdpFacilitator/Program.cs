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

            if (options.UseDefaultRdp && !string.IsNullOrWhiteSpace(options.RdpFilePath))
            {
                Console.WriteLine("Options -d (useDefaultRdp) and -f (rdpFile) cannot be used together.");
                return;
            }

            var rdpFilePath = options.UseDefaultRdp
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Default.rdp")
                : options.RdpFilePath;

            if (options.SelectedMonitors == null || !options.SelectedMonitors.Any() || string.IsNullOrWhiteSpace(rdpFilePath))
            {
                Console.WriteLine("Options -m (monitors) and -f (rdpFile) or -d (useDefaultRdp) are required unless -l is used.");
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

            var fileUpdater = new FileUpdater(rdpFilePath, options.SaveMode, selectedMonitors, options.Address);

            string rdpFileName;
            string? address;
            try
            {
                rdpFileName = fileUpdater.UpdateRdpFile();
                address = fileUpdater.GetAddress();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (options.Verbose)
            {
                Console.WriteLine($"RDP file : {rdpFileName}");
            }

            if (options.NoLaunch)
            {
                return;
            }

            if (options.UseDefaultRdp)
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    Console.WriteLine($"No remote computer address found in {rdpFileName}. Use -a to give it.");
                    return;
                }

                RdpLauncher.FromAddress(address).StartRemoteSession();
            }
            else
            {
                RdpLauncher.FromFile(rdpFileName).StartRemoteSession();
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
