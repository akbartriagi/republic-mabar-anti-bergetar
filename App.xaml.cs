using RecoilHelper.Core;
using System.Windows;

namespace RecoilHelper;

public partial class App : Application
{
    public static RecoilEngine Engine { get; } = new RecoilEngine();
    public static HotkeyManager Hotkey { get; private set; } = null!;
    public static DiscordRpcManager Discord { get; } = new DiscordRpcManager();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Load settings
        AppState.Settings = AppSettings.Load();
        AppState.RecoilEnabled = false; // selalu mulai mati

        Hotkey = new HotkeyManager(Engine);
        Discord.Initialize();

        Current.Exit += (_, _) =>
        {
            Hotkey.Dispose();
            Discord.Dispose(); // Tambah ini
            Engine.Dispose();
            AppState.Settings.Save();
        };
    }
}
