using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FortniteLauncher;

public enum GraphicsQualityProfile
{
    Rendimiento,
    Calidad
}

/// <summary>
/// Ajusta la calidad grafica de Fortnite editando las mismas claves de
/// "scalability" (sg.*) que el propio juego escribe en su archivo de
/// configuracion cuando cambiás la calidad desde el menu de video. No
/// modifica archivos del juego ni nada relacionado al anti-cheat.
/// </summary>
public static class GraphicsProfileManager
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FortniteGame", "Saved", "Config", "WindowsClient", "GameUserSettings.ini");

    private static readonly Dictionary<string, string> RendimientoValues = new()
    {
        ["sg.ViewDistanceQuality"] = "0",
        ["sg.AntiAliasingQuality"] = "0",
        ["sg.ShadowQuality"] = "0",
        ["sg.PostProcessQuality"] = "0",
        ["sg.TextureQuality"] = "0",
        ["sg.EffectsQuality"] = "0",
        ["sg.FoliageQuality"] = "0",
        ["sg.ShadingQuality"] = "0",
    };

    private static readonly Dictionary<string, string> CalidadValues = new()
    {
        ["sg.ViewDistanceQuality"] = "3",
        ["sg.AntiAliasingQuality"] = "3",
        ["sg.ShadowQuality"] = "3",
        ["sg.PostProcessQuality"] = "3",
        ["sg.TextureQuality"] = "3",
        ["sg.EffectsQuality"] = "3",
        ["sg.FoliageQuality"] = "3",
        ["sg.ShadingQuality"] = "3",
    };

    public static bool IsFortniteRunning() => Process.GetProcessesByName("FortniteClient-Win64-Shipping").Length > 0;

    public static bool Apply(GraphicsQualityProfile profile)
    {
        var values = profile == GraphicsQualityProfile.Rendimiento ? RendimientoValues : CalidadValues;

        try
        {
            var lines = File.Exists(ConfigPath)
                ? File.ReadAllLines(ConfigPath).ToList()
                : new List<string>();

            var sectionIndex = lines.FindIndex(l => l.Trim().Equals("[ScalabilityGroups]", StringComparison.OrdinalIgnoreCase));
            if (sectionIndex == -1)
            {
                lines.Add("[ScalabilityGroups]");
                sectionIndex = lines.Count - 1;
            }

            var sectionEnd = lines.FindIndex(sectionIndex + 1, l => l.TrimStart().StartsWith('['));
            if (sectionEnd == -1) sectionEnd = lines.Count;

            var pending = new Dictionary<string, string>(values);
            for (var i = sectionIndex + 1; i < sectionEnd; i++)
            {
                var eq = lines[i].IndexOf('=');
                if (eq <= 0) continue;
                var key = lines[i][..eq].Trim();
                if (pending.TryGetValue(key, out var newValue))
                {
                    lines[i] = $"{key}={newValue}";
                    pending.Remove(key);
                }
            }

            foreach (var kv in pending)
            {
                lines.Insert(sectionEnd, $"{kv.Key}={kv.Value}");
                sectionEnd++;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllLines(ConfigPath, lines);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
