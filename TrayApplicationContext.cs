using OneMoreGaugeRelay.Services;
using OneMoreGaugeRelay.Models;
using System.ComponentModel;

namespace OneMoreGaugeRelay;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly IRacingReader _iRacingReader;
    private readonly UdpBroadcaster _broadcaster;
    private readonly AppSettings _settings;
    private readonly System.Windows.Forms.Timer _updateTimer;

    private bool _isRunning = false;
    private ToolStripMenuItem _startStopItem = null!;
    private ToolStripMenuItem _statusItem = null!;

    public TrayApplicationContext()
    {
        _settings = AppSettings.Load();
        _iRacingReader = new IRacingReader();
        _broadcaster = new UdpBroadcaster(_settings.BroadcastPort);

        // Hook up iRacing connection events
        _iRacingReader.OnConnected += () => UpdateStatus(ConnectionStatus.Connected);
        _iRacingReader.OnDisconnected += () =>
        {
            if (_isRunning)
                UpdateStatus(ConnectionStatus.Waiting);
        };

        // Create context menu
        _contextMenu = new ContextMenuStrip();
        BuildContextMenu();

        // Create tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = GetStatusIcon(ConnectionStatus.Disconnected),
            ContextMenuStrip = _contextMenu,
            Visible = true,
            Text = "OneMoreGauge Relay"
        };

        _trayIcon.DoubleClick += (s, e) => ToggleRunning();

        // Update timer for telemetry broadcast
        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000 / _settings.UpdateRate // Convert Hz to ms
        };
        _updateTimer.Tick += OnUpdateTick;

        // Auto-start if configured
        if (_settings.AutoStart)
        {
            StartRelay();
        }
    }

    private void BuildContextMenu()
    {
        _statusItem = new ToolStripMenuItem("Status: Stopped")
        {
            Enabled = false
        };
        _contextMenu.Items.Add(_statusItem);
        _contextMenu.Items.Add(new ToolStripSeparator());

        _startStopItem = new ToolStripMenuItem("Start", null, (s, e) => ToggleRunning());
        _contextMenu.Items.Add(_startStopItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var settingsItem = new ToolStripMenuItem("Settings...", null, (s, e) => ShowSettings());
        _contextMenu.Items.Add(settingsItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());
        _contextMenu.Items.Add(exitItem);
    }

    private void ToggleRunning()
    {
        if (_isRunning)
        {
            StopRelay();
        }
        else
        {
            StartRelay();
        }
    }

    private void StartRelay()
    {
        _isRunning = true;
        _startStopItem.Text = "Stop";
        _iRacingReader.Start();
        _updateTimer.Start();
        UpdateStatus(ConnectionStatus.Waiting);
    }

    private void StopRelay()
    {
        _isRunning = false;
        _startStopItem.Text = "Start";
        _updateTimer.Stop();
        _iRacingReader.Stop();
        UpdateStatus(ConnectionStatus.Disconnected);
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        if (!_isRunning) return;

        var telemetry = _iRacingReader.GetTelemetry();

        if (telemetry != null)
        {
            UpdateStatus(ConnectionStatus.Connected);
            var packet = TelemetryPacket.Encode(telemetry);
            _broadcaster.Send(packet);
        }
        else
        {
            UpdateStatus(ConnectionStatus.Waiting);
        }
    }

    private void UpdateStatus(ConnectionStatus status)
    {
        _trayIcon.Icon = GetStatusIcon(status);
        _statusItem.Text = status switch
        {
            ConnectionStatus.Connected => "Status: Connected to iRacing",
            ConnectionStatus.Waiting => "Status: Waiting for iRacing...",
            ConnectionStatus.Disconnected => "Status: Stopped",
            _ => "Status: Unknown"
        };

        _trayIcon.Text = $"OneMoreGauge Relay - {_statusItem.Text.Replace("Status: ", "")}";
    }

    private static Icon GetStatusIcon(ConnectionStatus status)
    {
        // For now, use system icons. In production, embed custom icons.
        // Green = connected, Yellow = waiting, Red/Gray = disconnected
        return status switch
        {
            ConnectionStatus.Connected => SystemIcons.Application,
            ConnectionStatus.Waiting => SystemIcons.Warning,
            _ => SystemIcons.Shield
        };
    }

    private void ShowSettings()
    {
        using var dialog = new SettingsDialog(_settings);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _settings.Save();
            _broadcaster.UpdatePort(_settings.BroadcastPort);
            _updateTimer.Interval = 1000 / _settings.UpdateRate;
        }
    }

    private void ExitApplication()
    {
        StopRelay();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _iRacingReader.Dispose();
        _broadcaster.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
            _updateTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}

public enum ConnectionStatus
{
    Disconnected,
    Waiting,
    Connected
}
