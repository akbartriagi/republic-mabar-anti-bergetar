using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RecoilHelper.Core;

namespace RecoilHelper.Pages;

public partial class AboutPage : Page
{
    private const string DiscordUrl = "https://discord.gg/k4Gue7NRzD";

    public AboutPage()
    {
        InitializeComponent();
        
        AppState.LanguageChanged += OnLanguageChanged;
        TranslateUI();

        Unloaded += (_, _) => AppState.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(string _) => Dispatcher.Invoke(TranslateUI);

    private void TranslateUI()
    {
        if (TxtAppDesc != null) TxtAppDesc.Text = Lang.Get(
            "Aplikasi recoil helper portable yang dirancang untuk komunitas Republic Mabar. Dilengkapi dengan 3-stage recoil engine.", 
            "A portable recoil helper application designed for the Republic Mabar community. Equipped with a 3-stage recoil engine.",
            "تطبيق مساعد ارتداد محمول مصمم لمجتمع Republic Mabar. مجهز بمحرك ارتداد من 3 مراحل.");
            
        if (TxtFooter != null) TxtFooter.Text = Lang.Get(
            "Developed by lonetzy with ❤️ for Republic Mabar", 
            "Developed by lonetzy with ❤️ for Republic Mabar", 
            "تم التطوير بواسطة lonetzy بـ ❤️ من أجل Republic Mabar");
    }

    private void BtnDiscord_Click(object sender, RoutedEventArgs e)
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

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            Lang.Get("Anda sudah menggunakan versi terbaru (v2.5.2).", "You are already using the latest version (v2.5.2).", "أنت تستخدم بالفعل أحدث إصدار (v2.5.2)."), 
            "Update", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
