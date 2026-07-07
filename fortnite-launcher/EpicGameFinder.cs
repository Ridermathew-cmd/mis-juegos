using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FortniteLauncher;

public record EpicGameInfo(
    string DisplayName,
    string AppName,
    string CatalogNamespace,
    string CatalogItemId,
    string InstallLocation)
{
    public string BuildLaunchUri(bool silent = true)
    {
        var silentParam = silent ? "&silent=true" : "";
        return $"com.epicgames.launcher://apps/{CatalogNamespace}%3A{CatalogItemId}%3A{AppName}?action=launch{silentParam}";
    }
}

public static class EpicGameFinder
{
    private const string ManifestsPath = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";

    public static EpicGameInfo? FindFortnite()
    {
        if (!Directory.Exists(ManifestsPath))
            return null;

        foreach (var file in Directory.EnumerateFiles(ManifestsPath, "*.item"))
        {
            try
            {
                using var stream = File.OpenRead(file);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;

                var displayName = root.TryGetProperty("DisplayName", out var dn) ? dn.GetString() ?? "" : "";
                if (!displayName.Contains("Fortnite", StringComparison.OrdinalIgnoreCase))
                    continue;

                var appName = root.GetProperty("AppName").GetString() ?? "";
                var catalogNamespace = root.GetProperty("CatalogNamespace").GetString() ?? "";
                var catalogItemId = root.GetProperty("CatalogItemId").GetString() ?? "";
                var installLocation = root.TryGetProperty("InstallLocation", out var il) ? il.GetString() ?? "" : "";

                return new EpicGameInfo(displayName, appName, catalogNamespace, catalogItemId, installLocation);
            }
            catch
            {
                // Manifiesto corrupto o ilegible: se ignora y se sigue buscando.
            }
        }

        return null;
    }

    public static bool IsEpicGamesLauncherInstalled()
    {
        var candidatePaths = new[]
        {
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe"),
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe"),
        };
        return candidatePaths.Any(File.Exists);
    }
}
