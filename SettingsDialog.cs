using OneMoreGaugeRelay.Models;

namespace OneMoreGaugeRelay;

/// <summary>
/// Simple settings dialog for configuring the relay
/// </summary>
public class SettingsDialog : Form
{
    private readonly AppSettings _settings;
    private NumericUpDown _portInput = null!;
    private NumericUpDown _rateInput = null!;
    private CheckBox _autoStartCheckbox = null!;
    private CheckBox _startWithWindowsCheckbox = null!;

    public SettingsDialog(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "OneMoreGauge Relay Settings";
        Size = new Size(350, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            RowCount = 5,
            ColumnCount = 2
        };

        // Port setting
        layout.Controls.Add(new Label { Text = "Broadcast Port:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        _portInput = new NumericUpDown
        {
            Minimum = 1024,
            Maximum = 65535,
            Width = 100
        };
        layout.Controls.Add(_portInput, 1, 0);

        // Update rate setting
        layout.Controls.Add(new Label { Text = "Update Rate (Hz):", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        _rateInput = new NumericUpDown
        {
            Minimum = 10,
            Maximum = 60,
            Width = 100
        };
        layout.Controls.Add(_rateInput, 1, 1);

        // Auto-start checkbox
        _autoStartCheckbox = new CheckBox
        {
            Text = "Auto-start when app opens",
            AutoSize = true
        };
        layout.Controls.Add(_autoStartCheckbox, 0, 2);
        layout.SetColumnSpan(_autoStartCheckbox, 2);

        // Start with Windows checkbox
        _startWithWindowsCheckbox = new CheckBox
        {
            Text = "Start with Windows",
            AutoSize = true
        };
        layout.Controls.Add(_startWithWindowsCheckbox, 0, 3);
        layout.SetColumnSpan(_startWithWindowsCheckbox, 2);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill
        };

        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        var saveButton = new Button { Text = "Save", DialogResult = DialogResult.OK };
        saveButton.Click += (s, e) => SaveSettings();

        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(saveButton);
        layout.Controls.Add(buttonPanel, 0, 4);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadSettings()
    {
        _portInput.Value = _settings.BroadcastPort;
        _rateInput.Value = _settings.UpdateRate;
        _autoStartCheckbox.Checked = _settings.AutoStart;
        _startWithWindowsCheckbox.Checked = _settings.StartWithWindows;
    }

    private void SaveSettings()
    {
        _settings.BroadcastPort = (int)_portInput.Value;
        _settings.UpdateRate = (int)_rateInput.Value;
        _settings.AutoStart = _autoStartCheckbox.Checked;
        _settings.StartWithWindows = _startWithWindowsCheckbox.Checked;

        // Handle start with Windows
        SetStartWithWindows(_settings.StartWithWindows);
    }

    private static void SetStartWithWindows(bool enabled)
    {
        try
        {
            var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath, true);

            if (key == null) return;

            if (enabled)
            {
                var exePath = Application.ExecutablePath;
                key.SetValue("OneMoreGaugeRelay", $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue("OneMoreGaugeRelay", false);
            }
        }
        catch
        {
            // Silently fail if we can't modify registry
        }
    }
}
