using OneMoreGaugeRelay;

namespace OneMoreGaugeRelay;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Prevent multiple instances
        using var mutex = new Mutex(true, "OneMoreGaugeRelay", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("OneMoreGauge Relay is already running.", "Already Running",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
