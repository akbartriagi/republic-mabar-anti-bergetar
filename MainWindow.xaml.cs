using RecoilHelper.Core;
using RecoilHelper.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RecoilHelper;

public partial class MainWindow : Window
{
    private bool _alwaysOnTop = false;

    public MainWindow()
    {
        InitializeComponent();
        AppState.RecoilStatusChanged += OnRecoilStatusChanged;
        AppState.LanguageChanged += _ => Dispatcher.Invoke(TranslateUI);

        TranslateUI();

        // Tampilkan Splash Screen setiap peluncuran
        MainFrame.Navigate(new DiscordPage());
    }

    private void TranslateUI()
    {
        if (TxtNavDashboard != null) TxtNavDashboard.Text = Lang.Get("Dashboard", "Dashboard", "لوحة القيادة");
        if (TxtNavSettings != null) TxtNavSettings.Text = Lang.Get("Pengaturan", "Settings", "إعدادات");
        if (TxtNavAbout != null) TxtNavAbout.Text = Lang.Get("Tentang", "About", "حول");
        if (TxtStatusLabel != null) TxtStatusLabel.Text = Lang.Get("STATUS", "STATUS", "حالة");
        
        if (TxtAOT != null)
        {
            TxtAOT.Text = _alwaysOnTop 
                ? Lang.Get("Selalu di Atas ✓", "Always on Top ✓", "دائما في الأعلى ✓")
                : Lang.Get("Selalu di Atas", "Always on Top", "دائما في الأعلى");
        }

        OnRecoilStatusChanged(AppState.RecoilEnabled);
    }

    public void UnlockApp()
    {
        // Kembalikan ke layout standar
        TopBarBorder.Visibility = Visibility.Visible;
        SidebarBorder.Visibility = Visibility.Visible;

        Grid.SetRow(MainFrame, 1);
        Grid.SetColumn(MainFrame, 1);
        Grid.SetRowSpan(MainFrame, 1);
        Grid.SetColumnSpan(MainFrame, 1);

        NavigateTo("Dashboard");
    }

    private void BtnDashboard_Click(object sender, RoutedEventArgs e) => NavigateTo("Dashboard");
    private void BtnSettings_Click(object sender, RoutedEventArgs e) => NavigateTo("Settings");
    private void BtnAbout_Click(object sender, RoutedEventArgs e) => NavigateTo("About");

    private void NavigateTo(string page)
    {
        // Update button styles
        BtnDashboard.Style = (Style)FindResource(page == "Dashboard" ? "NavButtonActiveStyle" : "NavButtonStyle");
        BtnSettings.Style = (Style)FindResource(page == "Settings" ? "NavButtonActiveStyle" : "NavButtonStyle");
        BtnAbout.Style = (Style)FindResource(page == "About" ? "NavButtonActiveStyle" : "NavButtonStyle");

        switch (page)
        {
            case "Dashboard":
                MainFrame.Navigate(new Pages.DashboardPage());
                App.Discord.UpdatePresence("Dashboard", "Join discord.gg/k4Gue7NRzD");
                break;
            case "Settings":
                MainFrame.Navigate(new Pages.SettingsPage());
                App.Discord.UpdatePresence("Settings", "Join discord.gg/k4Gue7NRzD");
                break;
            case "About":
                MainFrame.Navigate(new Pages.AboutPage());
                App.Discord.UpdatePresence("About", "Join discord.gg/k4Gue7NRzD");
                break;
        }
    }

    private void BtnAlwaysOnTop_Click(object sender, RoutedEventArgs e)
    {
        _alwaysOnTop = !_alwaysOnTop;
        Topmost = _alwaysOnTop;
        BtnAlwaysOnTop.Style = (Style)FindResource(_alwaysOnTop ? "TopBarBtnActiveStyle" : "TopBarBtnStyle");
        TxtAOT.Text = _alwaysOnTop 
            ? Lang.Get("Selalu di Atas ✓", "Always on Top ✓", "دائما في الأعلى ✓") 
            : Lang.Get("Selalu di Atas", "Always on Top", "دائما في الأعلى");
    }

    private void OnRecoilStatusChanged(bool enabled)
    {
        Dispatcher.Invoke(() =>
        {
            StatusDot.Fill = new SolidColorBrush(enabled
                ? Color.FromRgb(0x39, 0xFF, 0x14)
                : Color.FromRgb(0x2A, 0x2A, 0x3A));
            StatusText.Text = enabled ? Lang.Get("AKTIF", "ACTIVE", "نشط") : Lang.Get("TIDAK AKTIF", "INACTIVE", "غير نشط");
            StatusText.Foreground = new SolidColorBrush(enabled
                ? Color.FromRgb(0x39, 0xFF, 0x14)
                : Color.FromRgb(0x3A, 0x3A, 0x5A));
        });
    }
}
