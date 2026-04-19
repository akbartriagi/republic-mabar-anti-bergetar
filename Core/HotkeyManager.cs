using System.Runtime.InteropServices;
using System.Windows.Input;

namespace RecoilHelper.Core;

public class HotkeyManager : IDisposable
{
    #region Win32

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT { public uint vkCode; public uint scanCode; public uint flags; public uint time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT { public System.Drawing.Point pt; public uint mouseData; public uint flags; public uint time; public IntPtr dwExtraInfo; }

    #endregion

    private IntPtr _kbHook = IntPtr.Zero;
    private IntPtr _msHook = IntPtr.Zero;
    private LowLevelKeyboardProc? _kbProc;
    private LowLevelMouseProc? _msProc;

    private readonly RecoilEngine _engine;
    private readonly HashSet<string> _pressedKeys = new();

    // Mode capture
    private string? _captureMode;
    private List<string> _captureBuffer = new();
    
    public event Action<string, string, bool>? KeyCaptured; // (mode, keyName, isFinal)
    public event Action<bool>? RecoilEngineToggle;

    public HotkeyManager(RecoilEngine engine)
    {
        _engine = engine;
        _kbProc = KeyboardProc;
        _msProc = MouseProc;

        var module = System.Diagnostics.Process.GetCurrentProcess().MainModule!;
        var hMod = GetModuleHandle(module.ModuleName!);
        _kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, _kbProc, hMod, 0);
        _msHook = SetWindowsHookEx(WH_MOUSE_LL, _msProc, hMod, 0);
    }

    public void StartCapture(string mode)
    {
        _captureMode = mode;
        _captureBuffer.Clear();
    }

    public void CancelCapture() => _captureMode = null;

    private bool IsHotkeyActive(string spec)
    {
        if (string.IsNullOrEmpty(spec)) return false;
        var parts = spec.Split(" + ");
        foreach (var p in parts)
        {
            if (!_pressedKeys.Contains(p)) return false;
        }
        return true;
    }

    private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var key = KeyInterop.KeyFromVirtualKey((int)kb.vkCode).ToString();
            int msg = (int)wParam;

            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                if (!_pressedKeys.Contains(key))
                {
                    _pressedKeys.Add(key);
                    ProcessInput(true, key);
                }
            }
            else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
            {
                if (_pressedKeys.Contains(key))
                {
                    _pressedKeys.Remove(key);
                    ProcessInput(false, key);
                }
            }

            if (_captureMode != null) return new IntPtr(1);
        }
        return CallNextHookEx(_kbHook, nCode, wParam, lParam);
    }

    private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var ms = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            int msg = (int)wParam;
            ushort hiword = (ushort)((ms.mouseData >> 16) & 0xFFFF);

            string? button = msg switch
            {
                WM_LBUTTONDOWN or WM_LBUTTONUP => "Mouse1",
                WM_RBUTTONDOWN or WM_RBUTTONUP => "Mouse2",
                WM_MBUTTONDOWN or WM_MBUTTONUP => "Mouse3",
                WM_XBUTTONDOWN or WM_XBUTTONUP => hiword == 1 ? "Mouse4" : "Mouse5",
                _ => null
            };

            if (button != null)
            {
                bool isDown = msg is WM_LBUTTONDOWN or WM_RBUTTONDOWN or WM_MBUTTONDOWN or WM_XBUTTONDOWN;
                if (isDown)
                {
                    if (!_pressedKeys.Contains(button))
                    {
                        _pressedKeys.Add(button);
                        ProcessInput(true, button);
                    }
                }
                else
                {
                    if (_pressedKeys.Contains(button))
                    {
                        _pressedKeys.Remove(button);
                        ProcessInput(false, button);
                    }
                }
            }

            if (_captureMode != null && button != null) return new IntPtr(1);
        }
        return CallNextHookEx(_msHook, nCode, wParam, lParam);
    }

    private void ProcessInput(bool isDown, string key)
    {
        if (_captureMode != null)
        {
            if (isDown)
            {
                if (!_captureBuffer.Contains(key))
                {
                    _captureBuffer.Add(key);
                    // Update preview real-time
                    KeyCaptured?.Invoke(_captureMode, string.Join(" + ", _captureBuffer), false);
                }
            }
            else
            {
                // Finalize when all keys are released
                if (_pressedKeys.Count == 0 && _captureBuffer.Count > 0)
                {
                    var mode = _captureMode;
                    var finalKey = string.Join(" + ", _captureBuffer);
                    _captureMode = null;

                    if (mode == "toggle") AppState.Settings.ToggleKey = finalKey;
                    else if (mode == "activate") AppState.Settings.ActivateKey = finalKey;
                    else if (mode != null && mode.StartsWith("profile"))
                    {
                        if (int.TryParse(mode.Substring(7), out int idx) && idx >= 0 && idx < AppState.Settings.Profiles.Count)
                            AppState.Settings.Profiles[idx].SwitchKey = finalKey;
                    }
                    
                    AppState.Settings.Save();
                    KeyCaptured?.Invoke(mode, finalKey, true);
                }
            }
            return;
        }

        // --- Standard Logic ---
        if (isDown)
        {
            // Profile Switch check
            for (int i = 0; i < AppState.Settings.Profiles.Count; i++)
            {
                if (IsHotkeyActive(AppState.Settings.Profiles[i].SwitchKey))
                {
                    AppState.ChangeProfile(i);
                    // Optional: You could trigger a specific sound here if needed.
                }
            }

            // Toggle check (must be ALL keys in combo pressed)
            if (IsHotkeyActive(AppState.Settings.ToggleKey))
            {
                AppState.RecoilEnabled = !AppState.RecoilEnabled;
                if (!AppState.RecoilEnabled) _engine.Stop();
                RecoilEngineToggle?.Invoke(AppState.RecoilEnabled);
            }
            
            // Activate check
            if (AppState.RecoilEnabled && IsHotkeyActive(AppState.Settings.ActivateKey))
            {
                _engine.Start();
            }
        }
        else
        {
            // If any part of the activation combo is released, stop
            if (!IsHotkeyActive(AppState.Settings.ActivateKey))
            {
                _engine.Stop();
            }
        }
    }

    public void Dispose()
    {
        if (_kbHook != IntPtr.Zero) { UnhookWindowsHookEx(_kbHook); _kbHook = IntPtr.Zero; }
        if (_msHook != IntPtr.Zero) { UnhookWindowsHookEx(_msHook); _msHook = IntPtr.Zero; }
    }
}
