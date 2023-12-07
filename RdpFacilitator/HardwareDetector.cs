using System.Runtime.InteropServices;

namespace RdpFacilitator
{
    public static class HardwareDetector
    {
        /*
            // https://community.spiceworks.com/topic/2351552-using-rdp-on-multiple-monitors-what-are-the-monitor-ids-listed-in-mstsc-l
            // https://stackoverflow.com/questions/18832991/enumdisplaydevices-not-returning-anything
            // https://stackoverflow.com/questions/4958683/how-do-i-get-the-actual-monitor-name-as-seen-in-the-resolution-dialog
            // http://www.pinvoke.net/default.aspx/user32.EnumDisplayDevices
         */
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MONITORINFOEX
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }

        [Flags()]
        private enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            MONITORINFOEX mi = new MONITORINFOEX();
            mi.Size = Marshal.SizeOf(typeof(MONITORINFOEX));
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                if(Verboose)
                {
                    Console.WriteLine($"DeviceName    : {mi.DeviceName}");
                    Console.WriteLine($"Flags         : {mi.Flags}");
                    Console.WriteLine($"Left          : {mi.Monitor.Left}");
                    Console.WriteLine($"Top           : {mi.Monitor.Top}");
                    Console.WriteLine($"WorkArea.Top  : {mi.WorkArea.Top}");
                    Console.WriteLine($"WorkArea.Left : {mi.WorkArea.Left}");
                    Console.WriteLine($"WorkArea.Right: {mi.WorkArea.Right}");
                    Console.WriteLine($"Monitor.Bottom: {mi.Monitor.Bottom}");
                    Console.WriteLine();
                }

                DisplayInfos[mi.DeviceName] = new DetectedMonitor
                {
                    LeftMostPosition = mi.Monitor.Left,
                    TopMost = mi.Monitor.Top,
                    DeviceName = mi.DeviceName
                };
            }
            return true;
        }

        private static Dictionary<string, DetectedMonitor> DisplayInfos = new Dictionary<string, DetectedMonitor>();
        private static bool Verboose = false;

        public static IEnumerable<DetectedMonitor> GetDisplayInfos(bool verbose)
        {
            Verboose = verbose;

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);

            if(verbose)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
            }

            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf<DISPLAY_DEVICE>();

            DISPLAY_DEVICE monitor = new DISPLAY_DEVICE();
            monitor.cb = Marshal.SizeOf<DISPLAY_DEVICE>();

            for (uint id = 0; EnumDisplayDevices(null, id, ref device, 0); id++)
            {
                if (verbose)
                {
                    Console.WriteLine($"DeviceName  : {device.DeviceName}");
                    Console.WriteLine($"DeviceString: {device.DeviceString}");
                    Console.WriteLine($"StateFlags  : {device.StateFlags}");
                    Console.WriteLine($"DeviceID    : {device.DeviceID}");
                    Console.WriteLine($"DeviceKey   : {device.DeviceKey}");
                    //device.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
                    Console.WriteLine();
                }

                if (DisplayInfos.TryGetValue(device.DeviceName, out var value))
                {
                    value.DeviceNumber = device.DeviceKey.Split('\\').Last();
                }
            }

            return DisplayInfos.Values.ToList();
        }
    }
}
