using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FortniteLauncher;

public record ControllerInfo(string Name, bool IsXInput, bool LikelyCompatible);

/// <summary>
/// Detecta mandos conectados via XInput (Xbox y compatibles) y via la API
/// clasica de joystick de Windows (winmm), que cubre mandos PlayStation,
/// Switch Pro y genericos que Windows expone como dispositivo HID/joystick.
/// </summary>
public static class ControllerMonitor
{
    private const int XUserMaxCount = 4;
    private const int JoyMaxDevices = 16;
    private const int JoyErrNoError = 0;

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern int XInputGetState(int dwUserIndex, out XinputState pState);

    [StructLayout(LayoutKind.Sequential)]
    private struct XinputGamepad
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XinputState
    {
        public uint dwPacketNumber;
        public XinputGamepad Gamepad;
    }

    [DllImport("winmm.dll")]
    private static extern int joyGetNumDevs();

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int joyGetDevCaps(int uJoyID, ref JoyCaps pjc, int cbjc);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct JoyCaps
    {
        public ushort wMid;
        public ushort wPid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public int wXmin;
        public int wXmax;
        public int wYmin;
        public int wYmax;
        public int wZmin;
        public int wZmax;
        public int wNumButtons;
        public int wPeriodMin;
        public int wPeriodMax;
        public int wRmin;
        public int wRmax;
        public int wUmin;
        public int wUmax;
        public int wVmin;
        public int wVmax;
        public int wCaps;
        public int wMaxAxes;
        public int wNumAxes;
        public int wMaxButtons;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szRegKey;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szOEMVxD;
    }

    public static List<ControllerInfo> GetConnectedControllers()
    {
        var result = new List<ControllerInfo>();

        for (var i = 0; i < XUserMaxCount; i++)
        {
            try
            {
                if (XInputGetState(i, out _) == 0)
                {
                    result.Add(new ControllerInfo($"Mando {i + 1} (Xbox / XInput)", true, true));
                }
            }
            catch (DllNotFoundException)
            {
                break;
            }
        }

        try
        {
            var joyCount = Math.Min(joyGetNumDevs(), JoyMaxDevices);
            for (var i = 0; i < joyCount; i++)
            {
                var caps = new JoyCaps();
                var size = Marshal.SizeOf(typeof(JoyCaps));
                if (joyGetDevCaps(i, ref caps, size) == JoyErrNoError && !string.IsNullOrWhiteSpace(caps.szPname))
                {
                    var isXboxDuplicate = caps.szPname.Contains("XBOX", StringComparison.OrdinalIgnoreCase)
                        && result.Exists(c => c.IsXInput);
                    if (!isXboxDuplicate)
                    {
                        result.Add(new ControllerInfo($"{caps.szPname} (HID)", false, false));
                    }
                }
            }
        }
        catch (DllNotFoundException)
        {
            // winmm.dll no disponible: se omite la deteccion por joystick clasico.
        }

        return result;
    }
}
