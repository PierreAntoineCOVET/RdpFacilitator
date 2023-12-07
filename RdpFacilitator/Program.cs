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

            var displayInfos = HardwareDetector.GetDisplayInfos(options.Verbose);

            var monitorManager = new MonitorManager(displayInfos, options.SelectedMonitors, options.Verbose);
            if (options.DisplayMonitorGrid)
            {
                monitorManager.PrintDetectedMonitorGrid();
                return;
            }

            var selectedMontiros = monitorManager.GetSelectMonitors();

            if(options.Verbose)
            {
                Console.WriteLine($"Selected monitors : {selectedMontiros}");
            }

            var fileUpdater = new FileUpdater(options.RdpFilePath, options.SaveMode, selectedMontiros);
            var newFileName = fileUpdater.UpdateRdpFile();

            if (options.Execute && !string.IsNullOrWhiteSpace(newFileName))
            {
                var rdpLauncher = new RdpLauncher(newFileName);
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