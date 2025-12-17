using IRSDKSharper;
using OneMoreGaugeRelay.Models;

namespace OneMoreGaugeRelay.Services;

/// <summary>
/// Reads telemetry data from iRacing's shared memory using IRSDKSharper
/// </summary>
public class IRacingReader : IDisposable
{
    private readonly IRacingSdk _sdk;
    private TelemetryData? _latestData;
    private readonly object _lock = new();
    private bool _disposed = false;

    public bool IsConnected => _sdk.IsConnected;

    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public IRacingReader()
    {
        _sdk = new IRacingSdk();
        _sdk.OnConnected += HandleConnected;
        _sdk.OnDisconnected += HandleDisconnected;
        _sdk.OnTelemetryData += HandleTelemetryData;
        _sdk.UpdateInterval = 1; // Every frame for lowest latency
    }

    public void Start()
    {
        _sdk.Start();
    }

    public void Stop()
    {
        _sdk.Stop();
    }

    private void HandleConnected()
    {
        OnConnected?.Invoke();
    }

    private void HandleDisconnected()
    {
        lock (_lock)
        {
            _latestData = null;
        }
        OnDisconnected?.Invoke();
    }

    private void HandleTelemetryData()
    {
        try
        {
            var data = _sdk.Data;
            if (data == null) return;

            var telemetry = new TelemetryData
            {
                // Engine/Vehicle
                RPM = data.GetFloat("RPM"),
                MaxRPM = GetMaxRPM(data),
                Speed = data.GetFloat("Speed"),
                Gear = (sbyte)data.GetInt("Gear"),
                Throttle = data.GetFloat("Throttle"),
                Brake = data.GetFloat("Brake"),
                Clutch = data.GetFloat("Clutch"),

                // Fuel
                Fuel = data.GetFloat("FuelLevel"),
                FuelPerLap = CalculateFuelPerLap(data),

                // Timing
                LapTime = data.GetFloat("LapCurrentLapTime"),
                BestLapTime = data.GetFloat("LapBestLapTime"),
                Delta = data.GetFloat("LapDeltaToBestLap"),
                Lap = (short)data.GetInt("Lap"),

                // Position
                Position = (sbyte)data.GetInt("PlayerCarPosition"),
                TotalCars = GetTotalCars(data),

                // Session
                SessionTimeRemaining = data.GetFloat("SessionTimeRemain"),

                // Tire temps (L/M/R for each corner)
                TireTempLFL = data.GetFloat("LFtempCL"),
                TireTempLFM = data.GetFloat("LFtempCM"),
                TireTempLFR = data.GetFloat("LFtempCR"),
                TireTempRFL = data.GetFloat("RFtempCL"),
                TireTempRFM = data.GetFloat("RFtempCM"),
                TireTempRFR = data.GetFloat("RFtempCR"),
                TireTempLRL = data.GetFloat("LRtempCL"),
                TireTempLRM = data.GetFloat("LRtempCM"),
                TireTempLRR = data.GetFloat("LRtempCR"),
                TireTempRRL = data.GetFloat("RRtempCL"),
                TireTempRRM = data.GetFloat("RRtempCM"),
                TireTempRRR = data.GetFloat("RRtempCR"),

                // Tire pressures
                TirePressureLF = data.GetFloat("LFpressure"),
                TirePressureRF = data.GetFloat("RFpressure"),
                TirePressureLR = data.GetFloat("LRpressure"),
                TirePressureRR = data.GetFloat("RRpressure"),

                // Flags
                IsOnTrack = data.GetBool("IsOnTrack"),
                IsInPit = data.GetBool("OnPitRoad"),
                SessionActive = data.GetInt("SessionState") > 0
            };

            lock (_lock)
            {
                _latestData = telemetry;
            }
        }
        catch
        {
            // Ignore telemetry read errors
        }
    }

    private static float GetMaxRPM(IRacingSdkData data)
    {
        try
        {
            // Try to get redline from session info
            var driverInfo = data.SessionInfo?.DriverInfo;
            if (driverInfo != null)
            {
                return (float)driverInfo.DriverCarRedLine;
            }
        }
        catch { }

        // Fallback to a reasonable default
        return 8000f;
    }

    private static float CalculateFuelPerLap(IRacingSdkData data)
    {
        try
        {
            var fuelPerHour = data.GetFloat("FuelUsePerHour");
            if (fuelPerHour > 0)
            {
                var lastLapTime = data.GetFloat("LapLastLapTime");
                var lapTime = lastLapTime > 0 ? lastLapTime : 90f;
                return fuelPerHour / 3600f * lapTime;
            }
        }
        catch { }
        return 0f;
    }

    private static sbyte GetTotalCars(IRacingSdkData data)
    {
        try
        {
            var drivers = data.SessionInfo?.DriverInfo?.Drivers;
            if (drivers != null)
            {
                return (sbyte)drivers.Count;
            }
        }
        catch { }
        return 0;
    }

    /// <summary>
    /// Gets the current telemetry data from iRacing
    /// </summary>
    /// <returns>TelemetryData if connected, null otherwise</returns>
    public TelemetryData? GetTelemetry()
    {
        lock (_lock)
        {
            return _latestData;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sdk.OnConnected -= HandleConnected;
            _sdk.OnDisconnected -= HandleDisconnected;
            _sdk.OnTelemetryData -= HandleTelemetryData;
            _sdk.Stop();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
