# EfsTools

A command-line tool for accessing the EFS (Embedded File System) on Qualcomm modems over USB.

## Features

- Get device and EFS filesystem info
- Read, write, rename, and delete files on the device
- Create and delete directories
- List directory contents
- Download/upload entire directory trees between the device and computer
- Read and write modem configuration (NV items) as JSON
- Extract MBN (Modem configuration BiNary) files
- Capture modem logs and messages
- Serve the EFS as a WebDAV share

## Requirements

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- libusb (installed via your system package manager)
- A Qualcomm device in DIAG mode connected via USB

## Configuration

Settings are stored in `EfsTools.dll.config`:

| Parameter | Default | Description |
|---|---|---|
| `vid` | `05C6` | USB Vendor ID (hex) |
| `pid` | `auto` | USB Product ID (hex), or `auto` to scan |
| `password` | `FFFFFFFFFFFFFFFF` | DIAG password |
| `spc` | `000000` | Service Programming Code |
| `hdlcSendControlChar` | `False` | Send HDLC leading control character |
| `ignoreUnsupportedCommands` | `True` | Ignore unsupported QCDM commands |

## Usage

```
EfsTools <command> [options]
```

### Commands

**targetInfo** — Print device information (IMEI, firmware version, etc.)
```
EfsTools targetInfo
```

**efsInfo** — Print EFS filesystem parameters
```
EfsTools efsInfo
```

**readFile** — Read a file from the device
```
EfsTools readFile -i /nv/item_files/example -o ./example
```

**writeFile** — Write a file to the device
```
EfsTools writeFile -i ./example -o /nv/item_files/example
```

**renameFile** — Rename/move a file on the device
```
EfsTools renameFile -p /path/old -n /path/new
```

**deleteFile** — Delete a file on the device
```
EfsTools deleteFile -p /path/to/file
```

**createDirectory** — Create a directory on the device
```
EfsTools createDirectory -p /path/to/dir
```

**deleteDirectory** — Delete a directory on the device
```
EfsTools deleteDirectory -p /path/to/dir
```

**listDirectory** — List files and directories
```
EfsTools listDirectory -p / -r
```

**downloadDirectory** — Download a directory from the device
```
EfsTools downloadDirectory -i / -o ./backup
```

**uploadDirectory** — Upload a directory to the device
```
EfsTools uploadDirectory -i ./backup -o /
```

**getModemConfig** — Export modem configuration as JSON (from device or local EFS directory)
```
EfsTools getModemConfig -p ./config.json
EfsTools getModemConfig -i ./efs_backup -p ./config.json
```

**setModemConfig** — Apply modem configuration from JSON (to device or local EFS directory)
```
EfsTools setModemConfig -p ./config.json
EfsTools setModemConfig -p ./config.json -o ./efs_output
```

**extractMbn** — Extract an MBN file to a directory
```
EfsTools extractMbn -i mcfg_sw.mbn -p ./mcfg
```

**getLog** — Capture modem logs
```
EfsTools getLog -l IMS_MESSAGE
```

**webDavServer** — Start a WebDAV server exposing the EFS
```
EfsTools webDavServer -p 8888 -r 1
```

**help** — Show help for a command
```
EfsTools help createDirectory
```

**version** — Print version
```
EfsTools version
```

## License

[MIT](License.md)

### Third-party libraries

- [CommandLineParser](https://github.com/commandlineparser/commandline) — Giacomo Stelluti Scala & Contributors
- [NWebDav](https://github.com/ramondeklein/nwebdav) — Ramon de Klein
- [Newtonsoft.Json](https://www.newtonsoft.com/json) — James Newton-King
- [ELFSharp](http://elfsharp.hellsgate.pl) — Konrad Kruczyński et al.
- [LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet) — Travis Robinson, quamotion

The Qualcomm DIAG/QCDM protocol implementation is based on [libopenpst](https://github.com/openpst/libopenpst) by Gassan Idriss.
