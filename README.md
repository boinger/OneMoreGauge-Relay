# OneMoreGauge Relay

Windows companion app for [One More Gauge](https://github.com/boinger/OneMoreGauge) - broadcasts iRacing telemetry to iOS devices over your local network.

## Features

- **System tray application** - Runs quietly in the background
- **Auto-detection** - Automatically connects when iRacing starts
- **Low latency** - UDP broadcast at 60Hz for real-time telemetry
- **Zero configuration** - Works out of the box on most home networks

## Requirements

- Windows 10/11
- iRacing installed and running
- iOS device on the same WiFi network

## Installation

1. Download the latest release from the [Releases](https://github.com/boinger/OneMoreGauge-Relay/releases) page
2. Run `OneMoreGaugeRelay.exe`
3. The app will appear in your system tray

## Usage

1. Start the Relay app (it will appear in your system tray)
2. Launch iRacing
3. Open One More Gauge on your iOS device
4. The app will automatically connect

### System Tray Menu

- **Start/Stop** - Toggle telemetry broadcasting
- **Settings** - Configure port and update rate
- **Exit** - Close the application

## Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Broadcast Port | 20777 | UDP port for telemetry data |
| Update Rate | 60 Hz | How often to send telemetry (10-60) |
| Auto-start | On | Start broadcasting when app opens |
| Start with Windows | Off | Launch app on Windows startup |

## Building from Source

Requires .NET 8 SDK.

```bash
# Build
dotnet build

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin/Release/net8.0-windows/win-x64/publish/`.

## Protocol

The relay broadcasts binary UDP packets containing:
- Engine data (RPM, speed, gear, throttle/brake)
- Timing (lap time, best lap, delta)
- Tire temperatures and pressures
- Session info (position, fuel, flags)

See [TelemetryPacket.cs](Services/TelemetryPacket.cs) for the exact format.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Related

- [One More Gauge (iOS)](https://github.com/boinger/OneMoreGauge) - The iOS app that displays the telemetry
- [iRacing](https://www.iracing.com/) - The sim racing platform
- [iRacingSDK.Net](https://github.com/vipoo/iRacingSDK.Net) - The C# SDK used to read iRacing data
