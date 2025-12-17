using OneMoreGaugeRelay.Models;
using System.Text;

namespace OneMoreGaugeRelay.Services;

/// <summary>
/// Encodes telemetry data into binary packets for transmission
/// </summary>
public static class TelemetryPacket
{
    // Magic bytes for packet validation: "OMG1"
    private static readonly byte[] MagicBytes = { 0x4F, 0x4D, 0x47, 0x31 };

    /// <summary>
    /// Encodes telemetry data into a binary packet
    /// </summary>
    public static byte[] Encode(TelemetryData data)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Header (4 bytes)
        writer.Write(MagicBytes);

        // Core data
        writer.Write(data.RPM);
        writer.Write(data.MaxRPM);
        writer.Write(data.Speed);
        writer.Write(data.Gear);
        writer.Write(data.Throttle);
        writer.Write(data.Brake);
        writer.Write(data.Clutch);
        writer.Write(data.Fuel);
        writer.Write(data.FuelPerLap);
        writer.Write(data.LapTime);
        writer.Write(data.BestLapTime);
        writer.Write(data.Delta);
        writer.Write(data.Position);
        writer.Write(data.TotalCars);
        writer.Write(data.Lap);
        writer.Write(data.SessionTimeRemaining);

        // Tire temps (12 floats)
        writer.Write(data.TireTempLFL);
        writer.Write(data.TireTempLFM);
        writer.Write(data.TireTempLFR);
        writer.Write(data.TireTempRFL);
        writer.Write(data.TireTempRFM);
        writer.Write(data.TireTempRFR);
        writer.Write(data.TireTempLRL);
        writer.Write(data.TireTempLRM);
        writer.Write(data.TireTempLRR);
        writer.Write(data.TireTempRRL);
        writer.Write(data.TireTempRRM);
        writer.Write(data.TireTempRRR);

        // Tire pressures (4 floats)
        writer.Write(data.TirePressureLF);
        writer.Write(data.TirePressureRF);
        writer.Write(data.TirePressureLR);
        writer.Write(data.TirePressureRR);

        // Flags (1 byte)
        byte flags = 0;
        if (data.IsOnTrack) flags |= 0x01;
        if (data.IsInPit) flags |= 0x02;
        if (data.SessionActive) flags |= 0x04;
        writer.Write(flags);

        // Get payload for checksum calculation
        var payload = stream.ToArray();

        // Calculate and append CRC16 checksum
        ushort checksum = CalculateCRC16(payload);
        writer.Write(checksum);

        return stream.ToArray();
    }

    /// <summary>
    /// Calculates CRC16 checksum (same algorithm as iOS decoder)
    /// </summary>
    private static ushort CalculateCRC16(byte[] data)
    {
        ushort crc = 0xFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 1) != 0)
                {
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        return crc;
    }
}
