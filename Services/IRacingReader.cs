using iRacingSDK;
using OneMoreGaugeRelay.Models;

namespace OneMoreGaugeRelay.Services;

/// <summary>
/// Reads telemetry data from iRacing's shared memory
/// </summary>
public class IRacingReader : IDisposable
{
    private readonly iRacingConnection _connection;
    private bool _disposed = false;

    public IRacingReader()
    {
        _connection = new iRacingConnection();
    }

    /// <summary>
    /// Gets the current telemetry data from iRacing
    /// </summary>
    /// <returns>TelemetryData if connected, null otherwise</returns>
    public TelemetryData? GetTelemetry()
    {
        try
        {
            if (!_connection.IsConnected)
            {
                return null;
            }

            var data = _connection.GetDataFeed();
            if (data == null)
            {
                return null;
            }

            return new TelemetryData
            {
                // Engine/Vehicle
                RPM = data.Telemetry.RPM,
                MaxRPM = data.SessionData.DriverInfo.DriverCarRedLine,
                Speed = data.Telemetry.Speed,
                Gear = (sbyte)data.Telemetry.Gear,
                Throttle = data.Telemetry.Throttle,
                Brake = data.Telemetry.Brake,
                Clutch = data.Telemetry.Clutch,

                // Fuel
                Fuel = data.Telemetry.FuelLevel,
                FuelPerLap = data.Telemetry.FuelUsePerHour > 0
                    ? data.Telemetry.FuelUsePerHour / 3600 * (data.Telemetry.LapLastLapTime > 0 ? data.Telemetry.LapLastLapTime : 90)
                    : 0,

                // Timing
                LapTime = data.Telemetry.LapCurrentLapTime,
                BestLapTime = data.Telemetry.LapBestLapTime,
                Delta = data.Telemetry.LapDeltaToBestLap,
                Lap = (short)data.Telemetry.Lap,

                // Position
                Position = (sbyte)data.Telemetry.PlayerCarPosition,
                TotalCars = (sbyte)(data.SessionData.DriverInfo?.Drivers?.Length ?? 0),

                // Session
                SessionTimeRemaining = data.Telemetry.SessionTimeRemain,

                // Tire temps (L/M/R for each corner)
                TireTempLFL = data.Telemetry.LFtempCL,
                TireTempLFM = data.Telemetry.LFtempCM,
                TireTempLFR = data.Telemetry.LFtempCR,
                TireTempRFL = data.Telemetry.RFtempCL,
                TireTempRFM = data.Telemetry.RFtempCM,
                TireTempRFR = data.Telemetry.RFtempCR,
                TireTempLRL = data.Telemetry.LRtempCL,
                TireTempLRM = data.Telemetry.LRtempCM,
                TireTempLRR = data.Telemetry.LRtempCR,
                TireTempRRL = data.Telemetry.RRtempCL,
                TireTempRRM = data.Telemetry.RRtempCM,
                TireTempRRR = data.Telemetry.RRtempCR,

                // Tire pressures
                TirePressureLF = data.Telemetry.LFpressure,
                TirePressureRF = data.Telemetry.RFpressure,
                TirePressureLR = data.Telemetry.LRpressure,
                TirePressureRR = data.Telemetry.RRpressure,

                // Flags
                IsOnTrack = data.Telemetry.IsOnTrack,
                IsInPit = data.Telemetry.OnPitRoad,
                SessionActive = data.Telemetry.SessionState > 0
            };
        }
        catch (Exception)
        {
            // iRacing not running or connection lost
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
