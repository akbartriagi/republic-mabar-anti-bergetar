namespace RecoilHelper.Core;
using System;

public static class AppState
{
    public static AppSettings Settings { get; set; } = AppSettings.Load();

    public static event Action<bool>? RecoilStatusChanged;
    public static event Action<int>? ProfileChanged;

    private static bool _recoilEnabled;
    public static bool RecoilEnabled
    {
        get => _recoilEnabled;
        set
        {
            if (_recoilEnabled == value) return;
            _recoilEnabled = value;
            Settings.RecoilEnabled = value;
            RecoilStatusChanged?.Invoke(value);
            if (value) SoundManager.PlayToggleOn();
            else SoundManager.PlayToggleOff();
        }
    }

    public static void ChangeProfile(int index)
    {
        if (index >= 0 && index < Settings.Profiles.Count)
        {
            bool triggerEvent = Settings.SelectedProfileIndex != index;
            Settings.SelectedProfileIndex = index;
            Settings.Save();
            if (triggerEvent) ProfileChanged?.Invoke(index);
        }
    }

    public static event Action<string>? LanguageChanged;

    public static void ChangeLanguage(string lang)
    {
        if (Settings.Language != lang)
        {
            Settings.Language = lang;
            Settings.Save();
            LanguageChanged?.Invoke(lang);
        }
    }
}
