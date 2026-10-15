using CommandLine;

namespace RdpFacilitator
{
    public class CommandLineOption
    {
        [Option('v', "verbose", HelpText = "Set output mode to verbose.", Default = false)]
        public bool Verbose { get; set; }

        [Option(
            'm',
            "monitors",
            HelpText = "List of selected monitors to use for RDP."
                + " Format 'RowNumber:ColumnNumber[,RowNumber:ColumnNumber]' with both RowNumber and ColumnNumber starting from 0."
                + " Monitors must be contiguous.",
            Separator = ','
        )]
        public IEnumerable<string>? SelectedMonitors { get; set; }

        [Option('l', "list", HelpText =  "List detected monitor grid", Default = false)]
        public bool DisplayMonitorGrid { get; set; }

        [Option(
            't',
            "rowTolerance",
            HelpText = "Maximum vertical offset in pixels between the tops of monitors considered on the same row.",
            Default = 10
        )]
        public int RowTolerance { get; set; }

        [Option(
            's',
            "saveMode",
            HelpText = "(Default: One). Define the behavior before updating the given RDP file:"
                + " None: Create no save file."
                + " One: Create save file named {yourFileName}.save.rdp. Save file will be overridden on each launch."
                + " Incremental: Create save file named {yourFileName}.save.{iterationNumber}.rdp. Create a new file each time.",
            Default = SaveMode.One
        )]
        public SaveMode SaveMode { get; set; }

        [Option('f', "rdpFile", HelpText = "RDP file to update. Created if it does not exist (requires -a).")]
        public string? RdpFilePath { get; set; }

        [Option('a', "address", HelpText = "Remote computer address. Required to create a new RDP file, replaces the existing address otherwise.")]
        public string? Address { get; set; }

        [Option('n', "noLaunch", HelpText = "Only update the RDP file, do not launch remote desktop.", Default = false)]
        public bool NoLaunch { get; set; }
    }

    public enum SaveMode
    {
        None,
        One,
        Incremental
    }
}
