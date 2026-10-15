# RdpFacilitator
Auto select RDP screens
Read the monitors detected by windows via registry and update rdp file to match.

**Beware : very little test has been done. It works for me and that's it.**

Monitors to use are passed down via a grid system based on extended display placement. Again no real tests on this.

Command line args I use on my side : -v -m 0:1,0:2 -f "RemoteDesktop.rdp" -s Incremental

Use command -h to display full command options.

## Examples

A relative `-f` path is resolved against the current folder, not the executable folder.

List the detected monitor grid, to find the `row:column` positions to use with `-m`:
```powershell
.\RdpFacilitator.exe -l
```

From the executable folder, create (if needed) `my-remote-host.rdp` and connect to `my-remote-host` using monitors `0:0` and `0:1`:
```powershell
.\RdpFacilitator.exe -m 0:0,0:1 -f my-remote-host.rdp -a my-remote-host
```

Same from any folder, using full paths:
```powershell
$dir = "C:\path\to\RdpFacilitator"
& "$dir\RdpFacilitator.exe" -m 0:0,0:1 -f "$dir\my-remote-host.rdp" -a my-remote-host
```

- First run: the RDP file is created with the address, `use multimon:i:1` and the selected monitors, then remote desktop is launched.
- Next runs: with the default `-s One`, the current file is first copied to `my-remote-host.save.rdp`, then updated and launched. Use `-s None` to skip the backup, or `-s Incremental` to keep every version (`my-remote-host.save.1.rdp`, `my-remote-host.save.2.rdp`, ...).
- `-a` is required only when the file does not exist yet; on an existing file it replaces the address.
- Add `-v` for verbose output, or `-n` to only create/update the file without connecting.
