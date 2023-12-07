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
            Separator = ',',
            Required = true
        )]
        public required IEnumerable<string> SelectedMonitors { get; set; }

        [Option('l', "list", HelpText =  "List detected monitor grid", Default = false)]
        public bool DisplayMonitorGrid { get; set; }

        [Option(
            's',
            "saveMode",
            HelpText = "(Default: One). Define the behavior before updating the given RDP file:"
                + " None: Create no save file."
                + " One: Create save file named {yourFileName}.save.rdp. Save file will be overriden on each launch."
                + " Incremental: Create save file named {yourFileName}.{iterationNumber}.save.rdp. Create a new file each time.",
            Default = SaveMode.One
        )]
        public SaveMode SaveMode { get; set; }

        [Option('f', "rdpFile", HelpText = "RDP file (full path) to update.", Required = true)]
        public required string RdpFilePath { get; set; }

        [Option('e', "execute", HelpText = "Launch remote desktop on new file", Required = false, Default = true)]
        public bool Execute { get; set; }
    }

    public enum SaveMode
    {
        None,
        One,
        Incremental
    }
}
