using System.Runtime.InteropServices;

namespace RecoilHelper.Core;

/// <summary>
/// Loop berjalan hanya ketika IsActivated = true (tombol aktivasi ditahan).
/// Toggle (RecoilEnabled) hanya sebagai "izin" — tidak langsung menggerakkan mouse.
/// </summary>
public class RecoilEngine : IDisposable
{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_MOVE = 0x0001;

    private CancellationTokenSource? _cts;
    private bool _loopRunning;

    // Diset true saat tombol aktivasi ditekan, false saat dilepas
    public bool IsActivated { get; private set; }

    public void Activate()
    {
        if (!AppState.RecoilEnabled) return; // toggle harus ON dulu
        IsActivated = true;
        if (_loopRunning) return;
        _cts = new CancellationTokenSource();
        _loopRunning = true;
        _ = Task.Run(() => RecoilLoop(_cts.Token));
    }

    public void Deactivate()
    {
        IsActivated = false;
        _cts?.Cancel();
        _loopRunning = false;
    }

    // Alias untuk kompatibilitas HotkeyManager
    public void Start() => Activate();
    public void Stop() => Deactivate();

    private async Task RecoilLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && IsActivated)
        {
            var settings = AppState.Settings;
            var prof = settings.Profiles[settings.SelectedProfileIndex];
            await RunEntry(prof.Entry1, token);
            if (!IsActivated || token.IsCancellationRequested) break;
            await RunEntry(prof.Entry2, token);
            if (!IsActivated || token.IsCancellationRequested) break;
            await RunEntry(prof.Entry3, token);
        }
        _loopRunning = false;
        IsActivated = false;
    }

    private async Task RunEntry(RecoilEntry entry, CancellationToken token)
    {
        if (entry.Duration <= 0) return;

        int moveIntervalMs = 10;
        double totalCycles = entry.Duration / (double)moveIntervalMs;
        if (totalCycles < 1) totalCycles = 1;

        int dx = (int)Math.Round(entry.Horizontal / totalCycles);
        int dy = (int)Math.Round(entry.VerticalSpeed / totalCycles * moveIntervalMs);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < entry.Duration && IsActivated && !token.IsCancellationRequested)
        {
            mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, IntPtr.Zero);
            await Task.Delay(moveIntervalMs, token).ContinueWith(_ => { });
        }
    }

    public void Dispose()
    {
        Deactivate();
        _cts?.Dispose();
    }
}
