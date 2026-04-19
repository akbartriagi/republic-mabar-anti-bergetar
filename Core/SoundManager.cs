using System.Media;
using System.Runtime.InteropServices;

namespace RecoilHelper.Core;

public static class SoundManager
{
    [DllImport("winmm.dll")]
    private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_ASYNC = 0x0001;
    private const uint SND_ALIAS = 0x00010000;

    public static void PlayToggleOn()
    {
        if (!AppState.Settings.SoundEnabled) return;
        
        switch (AppState.Settings.SoundType)
        {
            case 1: PlaySound("SystemExclamation", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
            case 2: PlaySound("SystemHand", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
            default: PlaySound("SystemAsterisk", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
        }
    }

    public static void PlayToggleOff()
    {
        if (!AppState.Settings.SoundEnabled) return;

        switch (AppState.Settings.SoundType)
        {
            case 1: PlaySound("SystemQuestion", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
            case 2: PlaySound("SystemExit", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
            default: PlaySound("SystemBeep", IntPtr.Zero, SND_ASYNC | SND_ALIAS); break;
        }
    }
}
