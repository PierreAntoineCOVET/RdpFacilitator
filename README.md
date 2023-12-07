# RdpFacilitator
Auto select RDP screens
Read the monitors detected by windows via registry and update rdp file to match.

`#FF0000` Beware : very little test has been done. It works for me and that's it.

Monitors to use are passed down via a grid system based on extended display placement. Again no real tests on this.

Command line args I use on my side : -v -m 0:1,0:2 -f "RemoteDesktop.rdp" -s Incremental

Use command -h to display full command options.
