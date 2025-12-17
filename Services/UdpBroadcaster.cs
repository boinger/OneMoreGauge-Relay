using System.Net;
using System.Net.Sockets;

namespace OneMoreGaugeRelay.Services;

/// <summary>
/// Broadcasts UDP packets to the local network
/// </summary>
public class UdpBroadcaster : IDisposable
{
    private UdpClient _client;
    private IPEndPoint _broadcastEndpoint;
    private bool _disposed = false;

    public UdpBroadcaster(int port = 20777)
    {
        _client = new UdpClient();
        _client.EnableBroadcast = true;
        _broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
    }

    /// <summary>
    /// Updates the broadcast port
    /// </summary>
    public void UpdatePort(int port)
    {
        _broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
    }

    /// <summary>
    /// Sends a packet to all devices on the local network
    /// </summary>
    public void Send(byte[] packet)
    {
        try
        {
            _client.Send(packet, packet.Length, _broadcastEndpoint);
        }
        catch (SocketException)
        {
            // Network error - ignore and try again next tick
        }
    }

    /// <summary>
    /// Sends a packet to a specific IP address (unicast mode)
    /// </summary>
    public void SendTo(byte[] packet, string ipAddress, int port)
    {
        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            _client.Send(packet, packet.Length, endpoint);
        }
        catch (Exception)
        {
            // Invalid IP or network error
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _client?.Close();
            _client?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
