using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace FortniteLauncher;

/// <summary>
/// Herramientas para mejorar el rendimiento en PCs de bajos recursos:
/// prioridad de proceso, plan de energia, liberacion de RAM y Modo Juego.
/// </summary>
public static class PerformanceTools
{
    private const string HighPerformanceGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    private const string UltimatePerformanceTemplateGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61";

    private static string UltimateSchemeCachePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FortniteLauncher", "ultimate_scheme.txt");

    public static readonly string[] KnownHeavyProcessNames =
    {
        "chrome", "msedge", "firefox", "opera", "brave",
        "Discord", "Spotify", "Teams", "slack", "OneDrive", "Skype"
    };

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    public static string? GetActiveSchemeGuid()
    {
        var output = RunPowercfg("/getactivescheme");
        var match = Regex.Match(output, @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");
        return match.Success ? match.Value : null;
    }

    public static bool HighPerformanceSchemeExists()
    {
        var output = RunPowercfg("/list");
        return output.Contains(HighPerformanceGuid, StringComparison.OrdinalIgnoreCase);
    }

    public static void EnableHighPerformancePlan()
    {
        if (HighPerformanceSchemeExists())
        {
            RunPowercfg($"/setactive {HighPerformanceGuid}");
        }
    }

    /// <summary>
    /// Intenta activar el plan oculto "Ultimate Performance" de Windows (mas
    /// agresivo que "Alto rendimiento", evita que Windows reduzca frecuencias
    /// de CPU para ahorrar energia). Si no esta disponible en este equipo,
    /// cae de vuelta al plan "Alto rendimiento" normal.
    /// </summary>
    public static void EnableMaxPerformancePlan()
    {
        var ultimateGuid = EnsureUltimatePerformanceScheme();
        if (ultimateGuid != null)
        {
            RunPowercfg($"/setactive {ultimateGuid}");
            return;
        }

        if (HighPerformanceSchemeExists())
        {
            RunPowercfg($"/setactive {HighPerformanceGuid}");
        }
    }

    private static string? EnsureUltimatePerformanceScheme()
    {
        try
        {
            if (File.Exists(UltimateSchemeCachePath))
            {
                var cachedGuid = File.ReadAllText(UltimateSchemeCachePath).Trim();
                if (!string.IsNullOrWhiteSpace(cachedGuid) &&
                    RunPowercfg("/list").Contains(cachedGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return cachedGuid;
                }
            }

            var output = RunPowercfg($"/duplicatescheme {UltimatePerformanceTemplateGuid}");
            var match = Regex.Match(output, @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");
            if (!match.Success) return null;

            var newGuid = match.Value;
            Directory.CreateDirectory(Path.GetDirectoryName(UltimateSchemeCachePath)!);
            File.WriteAllText(UltimateSchemeCachePath, newGuid);
            return newGuid;
        }
        catch
        {
            return null;
        }
    }

    public static void RestoreScheme(string? guid)
    {
        if (!string.IsNullOrWhiteSpace(guid))
        {
            RunPowercfg($"/setactive {guid}");
        }
    }

    private static string RunPowercfg(string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo("powercfg", arguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null) return "";
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);
            return output;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Vacia la memoria en espera de otros procesos del usuario para liberar RAM.
    /// Los procesos del sistema o sin permisos suficientes se ignoran.
    /// </summary>
    public static int TrimBackgroundProcesses()
    {
        var currentPid = Environment.ProcessId;
        var trimmed = 0;
        foreach (var process in Process.GetProcesses())
        {
            if (process.Id == currentPid) continue;
            try
            {
                if (EmptyWorkingSet(process.Handle))
                {
                    trimmed++;
                }
            }
            catch
            {
                // Procesos del sistema o sin permisos: se ignoran.
            }
        }
        return trimmed;
    }

    /// <summary>
    /// Pide a cada proceso que cierre su ventana principal (como si el usuario
    /// hiciera clic en la X), para que puedan guardar cambios antes de cerrar.
    /// </summary>
    public static int CloseProcesses(IEnumerable<string> processNames)
    {
        var closed = 0;
        foreach (var name in processNames)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                try
                {
                    if (process.CloseMainWindow())
                    {
                        closed++;
                    }
                }
                catch
                {
                    // Se ignora si el proceso no permite cerrarse asi.
                }
            }
        }
        return closed;
    }

    public static List<string> GetRunningHeavyProcessNames()
    {
        var running = new List<string>();
        foreach (var name in KnownHeavyProcessNames)
        {
            if (Process.GetProcessesByName(name).Length > 0)
            {
                running.Add(name);
            }
        }
        return running;
    }

    public static bool IsWindowsGameModeEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\GameBar");
            return key?.GetValue("AutoGameModeEnabled") is int i && i == 1;
        }
        catch
        {
            return false;
        }
    }

    public static void SetWindowsGameModeEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\GameBar");
        key.SetValue("AutoGameModeEnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    /// <summary>
    /// Game DVR es la grabacion en segundo plano de Xbox Game Bar. Consume
    /// CPU/RAM incluso sin estar grabando activamente; desactivarlo libera
    /// recursos en PCs de bajos recursos.
    /// </summary>
    public static bool IsGameDvrEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore");
            var value = key?.GetValue("GameDVR_Enabled");
            return !(value is int i && i == 0);
        }
        catch
        {
            return true;
        }
    }

    public static void SetGameDvrEnabled(bool enabled)
    {
        using (var key = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore"))
        {
            key.SetValue("GameDVR_Enabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
        {
            key.SetValue("AppCaptureEnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
        }
    }
}
