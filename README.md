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

### Row tolerance

Monitors whose top edges are at most 10 px apart (by default) are considered on the same row, so slightly misaligned monitors still end up on the same row of the grid. Use `-t` / `--rowTolerance` to change it, for example to allow up to 50 px:
```powershell
.\RdpFacilitator.exe -l -t 50
.\RdpFacilitator.exe -m 0:0,0:1 -f my-remote-host.rdp -a my-remote-host -t 50
```
Use `-t 0` to only group monitors whose top edges are exactly aligned.

### Launch from a desktop shortcut without a console window

RdpFacilitator is a console application, so Windows opens a console window when it starts. To hide it when using a desktop shortcut, start it through `conhost.exe --headless`. Shortcut properties:

- **Target**:
  ```
  C:\Windows\System32\conhost.exe --headless "C:\path\to\RdpFacilitator.exe" -m 0:0,0:1 -f "C:\path\to\my-remote-host.rdp" -a my-remote-host
  ```
- **Start in**: the executable folder, so relative `-f` paths are resolved next to the executable.
- Optionally use **Change Icon** and select `C:\Windows\System32\mstsc.exe`, as the shortcut shows the conhost icon otherwise.

In headless mode no output or error is visible: if nothing happens, run the same command (without `conhost.exe --headless`) in a terminal, with `-v`, to see what is wrong.

### Avoid the unknown publisher security warning

Opening an unsigned RDP file shows a Remote Desktop security warning ("Unknown remote connection", unknown publisher) on each connection. Connecting from the Remote Desktop window does not, because it uses `Default.rdp` from the Documents folder and opens no file.

Use `-d` / `--useDefaultRdp` to do the same: the selected monitors are written to `Default.rdp` in the Documents folder (instead of the `-f` file), then the connection is started with `mstsc /v:<address>`. The address comes from `-a`, or from the `full address` already stored in `Default.rdp`.
```powershell
.\RdpFacilitator.exe -m 0:0,0:1 -a my-remote-host -d
```
Desktop shortcut target:
```
C:\Windows\System32\conhost.exe --headless "C:\path\to\RdpFacilitator.exe" -m 0:0,0:1 -a my-remote-host -d
```
Connecting from the Remote Desktop window saves its own settings to `Default.rdp`, which may change the monitor selection; the next launch with `-d` writes it back.
