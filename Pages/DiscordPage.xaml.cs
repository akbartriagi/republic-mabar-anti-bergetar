using System;
using RecoilHelper.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RecoilHelper.Pages;

public partial class DiscordPage : Page
{
    private const string DiscordUrl = "https://discord.gg/k4Gue7NRzD";

    public DiscordPage()
    {
        InitializeComponent();
        
        // Listen to Discord RPC
        App.Discord.UserReady += OnUserReady;

        // Check if user info is already available (Race Condition fix)
        if (App.Discord.LastUser != null)
        {
            UpdateUserInfo(App.Discord.LastUser);
        }

        AppState.LanguageChanged += OnLanguageChanged;
        TranslateUI();

        Unloaded += (_, _) => 
        {
            App.Discord.UserReady -= OnUserReady;
            AppState.LanguageChanged -= OnLanguageChanged;
        };
    }

    private void OnLanguageChanged(string _) => Dispatcher.Invoke(TranslateUI);

    private void TranslateUI()
    {
        if (TxtWelcome != null && (TxtWelcome.Text.StartsWith("Halo,") || TxtWelcome.Text.StartsWith("Hello,") || TxtWelcome.Text.StartsWith("أهلاً،")))
        {
            var name = App.Discord.LastUser?.Name ?? "Gembeng";
            TxtWelcome.Text = Lang.Get($"Halo, {name}!", $"Hello, {name}!", $"أهلاً، {name}!");
        }

        if (TxtJoinTitle != null) TxtJoinTitle.Text = Lang.Get("Gabung discord Republic mabar", "Join Republic Mabar discord", "انضم إلى ديسكورد Republic Mabar");
        if (TxtJoinDesc != null) TxtJoinDesc.Text = Lang.Get("Dapatkan info update terbaru dan bergabung dengan komunitas kami.", "Get the latest update info and join our community.", "احصل على أحدث معلومات التحديث وانضم إلى مجتمعنا.");
        if (TxtBtnJoin != null) TxtBtnJoin.Text = Lang.Get("Gabung Discord Republic Mabar", "Join Republic Mabar Discord", "انضم إلى ديسكورد Republic Mabar");
        
        if (TxtBtnEnter != null) TxtBtnEnter.Text = Lang.Get("Masuk ke Aplikasi", "Enter Application", "الدخول إلى التطبيق");
        if (TxtFooterInfo != null) TxtFooterInfo.Text = Lang.Get("Informasi ini muncul setiap peluncuran aplikasi.", "This information appears every time the application is launched.", "تظهر هذه المعلومات في كل مرة يتم فيها تشغيل التطبيق.");
    }

    private void OnUserReady(object? sender, DiscordUserArgs e)
    {
        Dispatcher.Invoke(() => UpdateUserInfo(e));
    }

    private void UpdateUserInfo(DiscordUserArgs e)
    {
        TxtWelcome.Text = Lang.Get($"Halo, {e.Name}!", $"Hello, {e.Name}!", $"أهلاً، {e.Name}!");
        
        // Load Avatar
        if (!string.IsNullOrEmpty(e.AvatarUrl))
        {
            try {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(e.AvatarUrl));
                ImgAvatar.Source = bitmap;
                TxtDefaultIcon.Visibility = Visibility.Collapsed;
            } catch { }
        }
    }

    private void BtnJoin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Coba buka lewat aplikasi discord langsung
            Process.Start(new ProcessStartInfo("discord://discord.gg/k4Gue7NRzD") { UseShellExecute = true });
        }
        catch
        {
            try
            {
                // Fallback ke browser jika aplikasi tidak ada
                Process.Start(new ProcessStartInfo(DiscordUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka link: " + ex.Message);
            }
        }
    }

    private void BtnEnter_Click(object sender, RoutedEventArgs e)
    {
        // Jalankan animasi garasi ke atas
        var sb = TryFindResource("GarageDoorUp") as System.Windows.Media.Animation.Storyboard;
        if (sb != null)
        {
            sb.Completed += (s, ev) => UnlockSession();
            sb.Begin();
        }
        else
        {
            UnlockSession();
        }
    }

    private void UnlockSession()
    {
        var win = Window.GetWindow(this) as MainWindow;
        win?.UnlockApp();
    }
}
