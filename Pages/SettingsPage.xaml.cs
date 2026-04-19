using RecoilHelper.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RecoilHelper.Pages;

public partial class SettingsPage : Page
{
    private bool _loading = true;

    public SettingsPage()
    {
        InitializeComponent();
        
        // Atur ComboBox sebelum mengizinkan event tertrigger
        _loading = true;
        ComboLang.SelectedIndex = AppState.Settings.Language == "AR" ? 2 : (AppState.Settings.Language == "EN" ? 1 : 0);
        ComboSound.SelectedIndex = Math.Clamp(AppState.Settings.SoundType, 0, 2);
        _loading = false;

        LoadValues();
        TranslateUI();

        App.Hotkey.KeyCaptured += OnKeyCaptured;
        AppState.LanguageChanged += OnLanguageChanged;

        Unloaded += (_, _) =>
        {
            App.Hotkey.KeyCaptured -= OnKeyCaptured;
            AppState.LanguageChanged -= OnLanguageChanged;
        };
    }

    private void OnLanguageChanged(string _) => Dispatcher.Invoke(TranslateUI);

    private void ComboLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        if (ComboLang.SelectedItem is ComboBoxItem item && item.Tag is string lang)
        {
            AppState.ChangeLanguage(lang);
        }
    }

    private void ComboSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        if (ComboSound.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out var type))
        {
            AppState.Settings.SoundType = type;
            AppState.Settings.Save();
            SoundManager.PlayToggleOn();
        }
    }

    private void TranslateUI()
    {
        if (TxtSettingsTitle != null) TxtSettingsTitle.Text = Lang.Get("Pengaturan", "Settings", "إعدادات");
        if (TxtSettingsDesc != null) TxtSettingsDesc.Text = Lang.Get("Preferensi aplikasi dan hotkey", "Application and hotkey preferences", "تفضيلات التطبيق والاختصارات");
        
        if (TabGeneral != null) TabGeneral.Header = Lang.Get("  ⚙  Umum  ", "  ⚙  General  ", "  ⚙  عام  ");
        if (TabHotkey != null) TabHotkey.Header = Lang.Get("  ⌨  Pintasan  ", "  ⌨  Hotkey  ", "  ⌨  اختصار  ");

        // Tab General
        if (TxtSoundTitle != null) TxtSoundTitle.Text = Lang.Get("Efek Suara", "Sound Effect", "تأثير الصوت");
        if (TxtSoundDesc != null) TxtSoundDesc.Text = Lang.Get("Putar suara saat toggle recoil on/off", "Play sound when toggling recoil on/off", "تشغيل الصوت عند تشغيل/إيقاف الارتداد");
        if (TxtLangTitle != null) TxtLangTitle.Text = Lang.Get("Bahasa / Language", "Language", "اللغة");
        if (TxtLangDesc != null) TxtLangDesc.Text = Lang.Get("Pilih bahasa antarmuka", "Choose interface language", "اختر lghة الواجهة");

        if (ComboSound != null && ComboSound.Items.Count >= 3)
        {
            ((ComboBoxItem)ComboSound.Items[0]).Content = Lang.Get("Variasi 1", "Variation 1", "النوع 1");
            ((ComboBoxItem)ComboSound.Items[1]).Content = Lang.Get("Variasi 2", "Variation 2", "النوع 2");
            ((ComboBoxItem)ComboSound.Items[2]).Content = Lang.Get("Variasi 3", "Variation 3", "النوع 3");
        }

        // Tab Hotkey
        if (TxtHotkeyBanner != null) TxtHotkeyBanner.Text = Lang.Get("Klik tombol lalu tekan key atau mouse button yang diinginkan. Mendukung keyboard (F1–F12, dll.) dan mouse (Mouse1–Mouse5).", "Click a button then press your desired key or mouse button. Supports keyboard (F1-F12, etc.) and mouse (Mouse1-Mouse5).", "انقر فوق زر ثم اضغط pada المفتاح أو زر الماوس المطلوب. يدعم لوحة المفاتيح والماوس.");
        if (TxtToggleTitle != null) TxtToggleTitle.Text = Lang.Get("Toggle Recoil On/Off", "Toggle Recoil On/Off", "تشغيل/إيقاف الارتداد");
        if (TxtToggleDesc != null) TxtToggleDesc.Text = Lang.Get("Tekan sekali untuk nyalakan/matikan", "Press once to turn on/off", "اضغط مرة واحدة للتشغيل/الإيقاف");
        
        if (TxtActivateTitle != null) TxtActivateTitle.Text = Lang.Get("Aktifkan Recoil (Tahan)", "Activate Recoil (Hold)", "تفعيل الارتداد (استمرار)");
        if (TxtActivateDesc != null) TxtActivateDesc.Text = Lang.Get("Tahan tombol ini agar recoil bekerja", "Hold this button to make recoil work", "استمر في الضغط على هذا الزر لعمل الارتداد");

        if (TxtSwitchProfiles != null) TxtSwitchProfiles.Text = Lang.Get("GANTI PROFIL", "SWITCH PROFILES", "تبديل الملفات");
        if (BtnCancelCapture != null) BtnCancelCapture.Content = Lang.Get("Batal", "Cancel", "إلغاء");
    }

    private void LoadValues()
    {
        var s = AppState.Settings;
        ToggleSound.IsOn = s.SoundEnabled;
        ComboSound.SelectedIndex = Math.Clamp(s.SoundType, 0, 2);
        UpdateKeyButtons();
    }

    private void UpdateKeyButtons()
    {
        var s = AppState.Settings;
        BtnToggleKey.Content = $"[ {s.ToggleKey} ]";
        BtnActivateKey.Content = $"[ {s.ActivateKey} ]";
        
        BtnProfile1Key.Content = $"[ {s.Profiles[0].SwitchKey} ]";
        BtnProfile2Key.Content = $"[ {s.Profiles[1].SwitchKey} ]";
        BtnProfile3Key.Content = $"[ {s.Profiles[2].SwitchKey} ]";
        BtnProfile4Key.Content = $"[ {s.Profiles[3].SwitchKey} ]";
        BtnProfile5Key.Content = $"[ {s.Profiles[4].SwitchKey} ]";
    }

    private void ToggleSound_Toggled(object sender, RoutedEventArgs e)
    {
        AppState.Settings.SoundEnabled = ToggleSound.IsOn;
        AppState.Settings.Save();
    }

    // =================== HOTKEY CAPTURE ===================

    private void BtnToggleKey_Click(object sender, RoutedEventArgs e)
    {
        StartCapture("toggle", Lang.Get("Toggle Recoil On/Off", "Toggle Recoil On/Off", "تشغيل/إيقاف الارتداد"));
    }

    private void BtnActivateKey_Click(object sender, RoutedEventArgs e)
    {
        StartCapture("activate", Lang.Get("Aktifkan Recoil (Tahan)", "Activate Recoil (Hold)", "تفعيل الارتداد (استمرار)"));
    }

    private void BtnToggleKeyReset_Click(object sender, RoutedEventArgs e)
    {
        App.Hotkey.CancelCapture();
        AppState.Settings.ToggleKey = "F6";
        AppState.Settings.Save();
        UpdateKeyButtons();
        HideCaptureBanner();
    }

    private void BtnActivateKeyReset_Click(object sender, RoutedEventArgs e)
    {
        App.Hotkey.CancelCapture();
        AppState.Settings.ActivateKey = "Mouse1";
        AppState.Settings.Save();
        UpdateKeyButtons();
        HideCaptureBanner();
    }

    private void BtnCancelCapture_Click(object sender, RoutedEventArgs e)
    {
        App.Hotkey.CancelCapture();
        HideCaptureBanner();
    }

    private void BtnProfile1Key_Click(object sender, RoutedEventArgs e) => StartCapture("profile0", Lang.Get("Pindah ke Profile 1", "Switch to Profile 1", "التبديل إلى الملف 1"));
    private void BtnProfile2Key_Click(object sender, RoutedEventArgs e) => StartCapture("profile1", Lang.Get("Pindah ke Profile 2", "Switch to Profile 2", "التبديل إلى الملف 2"));
    private void BtnProfile3Key_Click(object sender, RoutedEventArgs e) => StartCapture("profile2", Lang.Get("Pindah ke Profile 3", "Switch to Profile 3", "التبديل إلى الملف 3"));
    private void BtnProfile4Key_Click(object sender, RoutedEventArgs e) => StartCapture("profile3", Lang.Get("Pindah ke Profile 4", "Switch to Profile 4", "التبديل إلى الملف 4"));
    private void BtnProfile5Key_Click(object sender, RoutedEventArgs e) => StartCapture("profile4", Lang.Get("Pindah ke Profile 5", "Switch to Profile 5", "التبديل إلى الملف 5"));

    private void ResetProfileKey(int index)
    {
        App.Hotkey.CancelCapture();
        AppState.Settings.Profiles[index].SwitchKey = "";
        AppState.Settings.Save();
        UpdateKeyButtons();
        HideCaptureBanner();
    }

    private void BtnProfile1KeyReset_Click(object sender, RoutedEventArgs e) => ResetProfileKey(0);
    private void BtnProfile2KeyReset_Click(object sender, RoutedEventArgs e) => ResetProfileKey(1);
    private void BtnProfile3KeyReset_Click(object sender, RoutedEventArgs e) => ResetProfileKey(2);
    private void BtnProfile4KeyReset_Click(object sender, RoutedEventArgs e) => ResetProfileKey(3);
    private void BtnProfile5KeyReset_Click(object sender, RoutedEventArgs e) => ResetProfileKey(4);

    private void StartCapture(string mode, string label)
    {
        var captureStr = Lang.Get("Capture untuk [{0}] — Tekan tombol keyboard atau mouse…", "Capture for [{0}] — Press keyboard or mouse button…", "التقاط لـ [{0}] - اضغط pada لوحة المفatيح أو الماوس ...");
        CaptureHintText.Text = $"🎯 {string.Format(captureStr, label)}";
        CaptureBanner.Visibility = Visibility.Visible;

        // Highlight active button
        var normalBrush = new SolidColorBrush(Colors.Gray);
        BtnToggleKey.Foreground = normalBrush;
        BtnActivateKey.Foreground = normalBrush;
        BtnProfile1Key.Foreground = normalBrush;
        BtnProfile2Key.Foreground = normalBrush;
        BtnProfile3Key.Foreground = normalBrush;
        BtnProfile4Key.Foreground = normalBrush;
        BtnProfile5Key.Foreground = normalBrush;

        var activeBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0xB0, 0xFF));
        if (mode == "toggle") BtnToggleKey.Foreground = activeBrush;
        else if (mode == "activate") BtnActivateKey.Foreground = activeBrush;
        else if (mode == "profile0") BtnProfile1Key.Foreground = activeBrush;
        else if (mode == "profile1") BtnProfile2Key.Foreground = activeBrush;
        else if (mode == "profile2") BtnProfile3Key.Foreground = activeBrush;
        else if (mode == "profile3") BtnProfile4Key.Foreground = activeBrush;
        else if (mode == "profile4") BtnProfile5Key.Foreground = activeBrush;

        App.Hotkey.StartCapture(mode);
    }

    private void OnKeyCaptured(string mode, string key, bool isFinal)
    {
        Dispatcher.Invoke(() =>
        {
            if (!isFinal)
            {
                // Real-time preview
                if (mode == "toggle") BtnToggleKey.Content = $"[ {key} ]";
                else if (mode == "activate") BtnActivateKey.Content = $"[ {key} ]";
                else if (mode == "profile0") BtnProfile1Key.Content = $"[ {key} ]";
                else if (mode == "profile1") BtnProfile2Key.Content = $"[ {key} ]";
                else if (mode == "profile2") BtnProfile3Key.Content = $"[ {key} ]";
                else if (mode == "profile3") BtnProfile4Key.Content = $"[ {key} ]";
                else if (mode == "profile4") BtnProfile5Key.Content = $"[ {key} ]";
                
                var recordStr = Lang.Get("Mencatat: {0} ... Lepaskan semua tombol untuk menyimpan.", "Recording: {0} ... Release all buttons to save.", "تسجيل: {0} ... حرر جميع الأزرار للحفظ.");
                CaptureHintText.Text = $"🎯 {string.Format(recordStr, key)}";
            }
            else
            {
                UpdateKeyButtons();
                HideCaptureBanner();
                var normalBrush = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xC0));
                BtnToggleKey.Foreground = normalBrush;
                BtnActivateKey.Foreground = normalBrush;
                BtnProfile1Key.Foreground = normalBrush;
                BtnProfile2Key.Foreground = normalBrush;
                BtnProfile3Key.Foreground = normalBrush;
                BtnProfile4Key.Foreground = normalBrush;
                BtnProfile5Key.Foreground = normalBrush;
            }
        });
    }

    private void HideCaptureBanner()
    {
        CaptureBanner.Visibility = Visibility.Collapsed;
    }
}
