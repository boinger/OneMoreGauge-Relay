namespace OneMoreGaugeRelay.Models;

/// <summary>
/// Represents telemetry data from iRacing
/// </summary>
public class TelemetryData
{
    // Engine/Vehicle
    public float RPM { get; set; }
    public float MaxRPM { get; set; }
    public float Speed { get; set; }  // m/s
    public sbyte Gear { get; set; }   // -1=R, 0=N, 1-8
    public float Throttle { get; set; }
    public float Brake { get; set; }
    public float Clutch { get; set; }

    // Fuel
    public float Fuel { get; set; }        // liters
    public float FuelPerLap { get; set; }  // liters per lap

    // Timing
    public float LapTime { get; set; }      // current lap time in seconds
    public float BestLapTime { get; set; }  // best lap time in seconds
    public float Delta { get; set; }        // delta vs best (negative = faster)
    public short Lap { get; set; }

    // Position
    public sbyte Position { get; set; }
    public sbyte TotalCars { get; set; }

    // Session
    public float SessionTimeRemaining { get; set; }

    // Tire temps (Celsius) - Left/Middle/Right across tread
    public float TireTempLFL { get; set; }
    public float TireTempLFM { get; set; }
    public float TireTempLFR { get; set; }
    public float TireTempRFL { get; set; }
    public float TireTempRFM { get; set; }
    public float TireTempRFR { get; set; }
    public float TireTempLRL { get; set; }
    public float TireTempLRM { get; set; }
    public float TireTempLRR { get; set; }
    public float TireTempRRL { get; set; }
    public float TireTempRRM { get; set; }
    public float TireTempRRR { get; set; }

    // Tire pressures (kPa)
    public float TirePressureLF { get; set; }
    public float TirePressureRF { get; set; }
    public float TirePressureLR { get; set; }
    public float TirePressureRR { get; set; }

    // Flags
    public bool IsOnTrack { get; set; }
    public bool IsInPit { get; set; }
    public bool SessionActive { get; set; }
}
