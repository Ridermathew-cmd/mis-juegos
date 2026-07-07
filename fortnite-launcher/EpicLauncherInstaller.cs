using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FortniteLauncher;

/// <summary>
/// Descarga el instalador oficial de Epic Games Launcher (mismo archivo que
/// sirve epicgames.com, alojado en su propio dominio) y lo ejecuta. La
/// instalacion en si la maneja el instalador real de Epic: pide elevacion de
/// Windows (UAC) y muestra sus propios terminos, no los saltamos.
/// </summary>
public static class EpicLauncherInstaller
{
    private const string InstallerUrl =
        "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(5) };

    public static async Task<bool> DownloadAndRunInstallerAsync()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "EpicGamesLauncherInstaller.msi");

            await using (var stream = await Http.GetStreamAsync(InstallerUrl))
            await using (var file = File.Create(tempPath))
            {
                await stream.CopyToAsync(file);
            }

            using var process = Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            if (process is null) return false;

            await process.WaitForExitAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
