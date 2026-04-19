using RecoilHelper.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RecoilHelper.Pages;

public partial class DashboardPage : Page
{
    private bool _loading = true;
    private readonly DispatcherTimer _saveTimer;

    public DashboardPage()
    {
        InitializeComponent();

        // Debounce: simpan 600ms setelah nilai terakhir diubah
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); AutoSave(); };

        AppState.LanguageChanged += OnLanguageChanged;
        AppState.RecoilStatusChanged += OnRecoilChanged;
        AppState.ProfileChanged += OnProfileChanged;

        Unloaded += (_, _) => 
        {
            AppState.LanguageChanged -= OnLanguageChanged;
            AppState.RecoilStatusChanged -= OnRecoilChanged;
            AppState.ProfileChanged -= OnProfileChanged;
        };

        TranslateUI();
        LoadValues();
        UpdateHotkeyHint();
    }

    private void OnLanguageChanged(string _) => Dispatcher.Invoke(TranslateUI);

    private void TranslateUI()
    {
        if (TxtDashTitle != null) TxtDashTitle.Text = Lang.Get("Dasbor", "Dashboard", "لوحة القيادة");
        if (TxtDashDesc != null) TxtDashDesc.Text = Lang.Get("Konfigurasi dan kontrol recoil", "Recoil configuration and control", "تكوين والتحكم في الارتداد");
        if (TxtRecoilControl != null) TxtRecoilControl.Text = Lang.Get("KONTROL RECOIL", "RECOIL CONTROL", "التحكم في الارتداد");
        
        if (BtnImport != null) BtnImport.ToolTip = Lang.Get("Import Profile", "Import Profile", "استيراد الملف المرجعي");
        if (BtnExport != null) BtnExport.ToolTip = Lang.Get("Export Profile", "Export Profile", "تصدير الملف المرجعي");
        if (TxtBtnImport != null) TxtBtnImport.Text = Lang.Get("Impor", "Import", "استيراد");
        if (TxtBtnExport != null) TxtBtnExport.Text = Lang.Get("Ekspor", "Export", "تصدير");
        
        if (EProfileNote != null) EProfileNote.ToolTip = Lang.Get("Catatan Profil", "Profile Note", "ملاحظة الملف المرجعي");
        
        UpdateComboBoxItems();
        UpdateHotkeyHint();
    }

    private void ComboProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        if (ComboProfile.SelectedIndex >= 0)
        {
            AppState.ChangeProfile(ComboProfile.SelectedIndex);
        }
    }

    private void OnProfileChanged(int newIndex)
    {
        Dispatcher.Invoke(() =>
        {
            LoadValues();
        });
    }

    private void UpdateComboBoxItems()
    {
        if (ComboProfile == null || AppState.Settings.Profiles.Count < 5) return;
        var s = AppState.Settings;
        string prefix = Lang.Get("Profil", "Profile", "الملف المرجعي");
        for (int i = 0; i < 5; i++)
        {
            if (ComboProfile.Items[i] is ComboBoxItem item)
            {
                string desc = s.Profiles[i].Description;
                item.Content = string.IsNullOrWhiteSpace(desc) ? $"{prefix} {i + 1}" : $"{prefix} {i + 1} - {desc}";
            }
        }
    }

    // =================== IMPORT / EXPORT ===================
    private void BtnImport_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            Title = Lang.Get("Import Profil", "Import Profile", "استيراد الملف المرجعي")
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);
                var imported = Newtonsoft.Json.JsonConvert.DeserializeObject<RecoilProfile>(json);
                if (imported != null)
                {
                    var current = AppState.Settings.Profiles[AppState.Settings.SelectedProfileIndex];
                    current.Description = imported.Description ?? "";
                    current.Entry1 = imported.Entry1 ?? new RecoilEntry();
                    current.Entry2 = imported.Entry2 ?? new RecoilEntry();
                    current.Entry3 = imported.Entry3 ?? new RecoilEntry();
                    
                    AppState.Settings.Save();
                    LoadValues();
                    MessageBox.Show(Lang.Get("Profil berhasil diimport!", "Profile successfully imported!", "تم استيراد الملف المرجعي بنجاح!"), "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        var prof = AppState.Settings.Profiles[AppState.Settings.SelectedProfileIndex];
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"Recoil_{prof.Name.Replace(" ", "")}.json",
            Filter = "JSON Files (*.json)|*.json",
            Title = Lang.Get("Export Profil", "Export Profile", "تصدير الملف المرجعي")
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var exportData = new {
                    Name = prof.Name,
                    Description = prof.Description,
                    Entry1 = prof.Entry1,
                    Entry2 = prof.Entry2,
                    Entry3 = prof.Entry3
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(exportData, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(dialog.FileName, json);
                MessageBox.Show(Lang.Get("Profil berhasil diexport!", "Profile successfully exported!", "تم تصدير الملف المرجعي بنجاح!"), "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // =================== LOAD ===================
    private void LoadValues()
    {
        _loading = true;
        var s = AppState.Settings;
        var prof = s.Profiles[s.SelectedProfileIndex];
        ToggleRecoil.IsOn = AppState.RecoilEnabled;

        ComboProfile.SelectedIndex = s.SelectedProfileIndex;
        UpdateComboBoxItems();

        EProfileNote.Text = prof.Description ?? "";

        SL1Vertical.Value   = prof.Entry1.VerticalSpeed;
        SL1Duration.Value   = prof.Entry1.Duration;
        SL1Horizontal.Value = prof.Entry1.Horizontal;
        E1Vertical.Text     = prof.Entry1.VerticalSpeed.ToString("F1");
        E1Duration.Text     = prof.Entry1.Duration.ToString();
        E1Horizontal.Text   = prof.Entry1.Horizontal.ToString("F1");

        SL2Vertical.Value   = prof.Entry2.VerticalSpeed;
        SL2Duration.Value   = prof.Entry2.Duration;
        SL2Horizontal.Value = prof.Entry2.Horizontal;
        E2Vertical.Text     = prof.Entry2.VerticalSpeed.ToString("F1");
        E2Duration.Text     = prof.Entry2.Duration.ToString();
        E2Horizontal.Text   = prof.Entry2.Horizontal.ToString("F1");

        SL3Vertical.Value   = prof.Entry3.VerticalSpeed;
        SL3Duration.Value   = prof.Entry3.Duration;
        SL3Horizontal.Value = prof.Entry3.Horizontal;
        E3Vertical.Text     = prof.Entry3.VerticalSpeed.ToString("F1");
        E3Duration.Text     = prof.Entry3.Duration.ToString();
        E3Horizontal.Text   = prof.Entry3.Horizontal.ToString("F1");

        _loading = false;
    }

    private void UpdateHotkeyHint()
    {
        var s = AppState.Settings;
        var tLbl = Lang.Get("Toggle", "Toggle", "تبديل");
        var aLbl = Lang.Get("Aktifkan (tahan)", "Activate (hold)", "تفعيل (استمرار)");
        if (TxtHotkeyHint != null) TxtHotkeyHint.Text = $"{tLbl}: [{s.ToggleKey}]   {aLbl}: [{s.ActivateKey}]";
    }

    // =================== AUTO-SAVE ===================
    private void ScheduleSave()
    {
        if (_loading) return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void AutoSave()
    {
        var s = AppState.Settings;
        var prof = s.Profiles[s.SelectedProfileIndex];
        prof.Description = EProfileNote.Text;
        if (double.TryParse(E1Vertical.Text, out var v))   prof.Entry1.VerticalSpeed = v;
        if (int.TryParse(E1Duration.Text, out var d))      prof.Entry1.Duration = d;
        if (double.TryParse(E1Horizontal.Text, out var h)) prof.Entry1.Horizontal = h;
        if (double.TryParse(E2Vertical.Text, out v))       prof.Entry2.VerticalSpeed = v;
        if (int.TryParse(E2Duration.Text, out d))          prof.Entry2.Duration = d;
        if (double.TryParse(E2Horizontal.Text, out h))     prof.Entry2.Horizontal = h;
        if (double.TryParse(E3Vertical.Text, out v))       prof.Entry3.VerticalSpeed = v;
        if (int.TryParse(E3Duration.Text, out d))          prof.Entry3.Duration = d;
        if (double.TryParse(E3Horizontal.Text, out h))     prof.Entry3.Horizontal = h;
        s.Save();
        UpdateComboBoxItems();
    }

    // =================== TOGGLE ===================
    private void ToggleRecoil_Toggled(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppState.RecoilEnabled = ToggleRecoil.IsOn;
        if (!AppState.RecoilEnabled) App.Engine.Deactivate();
    }

    private void OnRecoilChanged(bool enabled)
    {
        Dispatcher.Invoke(() =>
        {
            _loading = true;
            ToggleRecoil.IsOn = enabled;
            _loading = false;
        });
    }

    // =================== SLIDER → TEXTBOX ===================
    private void SL1Vertical_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E1Vertical.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }
    private void SL1Duration_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E1Duration.Text = ((int)e.NewValue).ToString(); _loading = false; ScheduleSave(); } }
    private void SL1Horizontal_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E1Horizontal.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }
    private void SL2Vertical_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E2Vertical.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }
    private void SL2Duration_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E2Duration.Text = ((int)e.NewValue).ToString(); _loading = false; ScheduleSave(); } }
    private void SL2Horizontal_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E2Horizontal.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }
    private void SL3Vertical_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E3Vertical.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }
    private void SL3Duration_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E3Duration.Text = ((int)e.NewValue).ToString(); _loading = false; ScheduleSave(); } }
    private void SL3Horizontal_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    { if (!_loading) { _loading = true; E3Horizontal.Text = e.NewValue.ToString("F1"); _loading = false; ScheduleSave(); } }

    // =================== TEXTBOX → SLIDER ===================
    private void UpdateSliderFromText(TextBox tb, Slider sl, bool isInt = false)
    {
        if (_loading) return;
        if (isInt) { if (int.TryParse(tb.Text, out var iv)) { _loading = true; sl.Value = iv; _loading = false; } }
        else { if (double.TryParse(tb.Text, out var dv)) { _loading = true; sl.Value = Math.Clamp(dv, sl.Minimum, sl.Maximum); _loading = false; } }
        ScheduleSave();
    }

    private void EProfileNote_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loading) ScheduleSave();
    }

    private void E1Vertical_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E1Vertical, SL1Vertical);
    private void E1Duration_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E1Duration, SL1Duration, true);
    private void E1Horizontal_TextChanged(object s, TextChangedEventArgs e) => UpdateSliderFromText(E1Horizontal, SL1Horizontal);
    private void E2Vertical_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E2Vertical, SL2Vertical);
    private void E2Duration_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E2Duration, SL2Duration, true);
    private void E2Horizontal_TextChanged(object s, TextChangedEventArgs e) => UpdateSliderFromText(E2Horizontal, SL2Horizontal);
    private void E3Vertical_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E3Vertical, SL3Vertical);
    private void E3Duration_TextChanged(object s, TextChangedEventArgs e)   => UpdateSliderFromText(E3Duration, SL3Duration, true);
    private void E3Horizontal_TextChanged(object s, TextChangedEventArgs e) => UpdateSliderFromText(E3Horizontal, SL3Horizontal);

    // =================== RESET ===================
    private void ResetEntry(
        TextBox tbV, TextBox tbD, TextBox tbH,
        Slider slV, Slider slD, Slider slH)
    {
        _loading = true;
        tbV.Text = "0.0"; slV.Value = 0;
        tbD.Text = "0";   slD.Value = 0;
        tbH.Text = "0.0"; slH.Value = 0;
        _loading = false;
        AutoSave(); // langsung simpan saat reset
    }

    private void BtnReset1_Click(object s, RoutedEventArgs e)
        => ResetEntry(E1Vertical, E1Duration, E1Horizontal, SL1Vertical, SL1Duration, SL1Horizontal);
    private void BtnReset2_Click(object s, RoutedEventArgs e)
        => ResetEntry(E2Vertical, E2Duration, E2Horizontal, SL2Vertical, SL2Duration, SL2Horizontal);
    private void BtnReset3_Click(object s, RoutedEventArgs e)
        => ResetEntry(E3Vertical, E3Duration, E3Horizontal, SL3Vertical, SL3Duration, SL3Horizontal);
}
