using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;

namespace RecoilHelper.Core;

public class RecoilEntry
{
    public double VerticalSpeed { get; set; } = 5.0;
    public int Duration { get; set; } = 100;
    public double Horizontal { get; set; } = 0.0;
}

public class RecoilProfile
{
    public string Name { get; set; } = "Profile 1";
    public string Description { get; set; } = "";
    public RecoilEntry Entry1 { get; set; } = new();
    public RecoilEntry Entry2 { get; set; } = new();
    public RecoilEntry Entry3 { get; set; } = new();
    public string SwitchKey { get; set; } = "";
}

public class AppSettings
{
    public bool RecoilEnabled { get; set; } = false;
    public bool SoundEnabled { get; set; } = true;
    public int SoundType { get; set; } = 0; // 0, 1, 2
    public string Language { get; set; } = "ID"; // "ID" atau "EN" atau "AR"
    public string ToggleKey { get; set; } = "F6";
    public string ActivateKey { get; set; } = "Mouse1";

    public List<RecoilProfile> Profiles { get; set; } = new();
    public int SelectedProfileIndex { get; set; } = 0;

    // Backward compatibility for old format
    public RecoilEntry? Entry1 { get; set; }
    public RecoilEntry? Entry2 { get; set; }
    public RecoilEntry? Entry3 { get; set; }

    public bool ShouldSerializeEntry1() => false;
    public bool ShouldSerializeEntry2() => false;
    public bool ShouldSerializeEntry3() => false;

    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RecoilHelper", "settings.json");

    public static AppSettings Load()
    {
        AppSettings settings = new AppSettings();
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }

        // Initialization & Migration
        if (settings.Profiles == null || settings.Profiles.Count < 5)
        {
            settings.Profiles = new List<RecoilProfile>();
            for (int i = 1; i <= 5; i++)
            {
                var p = new RecoilProfile { Name = $"Profile {i}" };
                
                // Migrate old Entries to Profile 1
                if (i == 1 && settings.Entry1 != null)
                {
                    p.Entry1 = settings.Entry1;
                    p.Entry2 = settings.Entry2 ?? new();
                    p.Entry3 = settings.Entry3 ?? new();
                }
                settings.Profiles.Add(p);
            }
        }

        settings.Entry1 = null;
        settings.Entry2 = null;
        settings.Entry3 = null;

        if (settings.SelectedProfileIndex < 0 || settings.SelectedProfileIndex >= settings.Profiles.Count)
            settings.SelectedProfileIndex = 0;

        return settings;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch { }
    }
}
