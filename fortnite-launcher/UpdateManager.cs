using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FortniteLauncher;

public record UpdateInfo(string Version, string DownloadUrl);

/// <summary>
/// Chequea si hay una version mas nueva publicada en version.json (en la
/// misma pagina de descarga) y, si hay, descarga el .zip y se auto-reemplaza:
/// copia los archivos nuevos encima de los actuales y se vuelve a abrir.
/// Necesita cerrarse a si misma primero para poder sobreescribir su propio
/// .exe (por eso el reemplazo lo hace un script externo, no el proceso
/// mismo).
/// </summary>
public static class UpdateManager
{
    public const string CurrentVersion = "1.2.0";

    private const string VersionInfoUrl =
        "https://ridermathew-cmd.github.io/mis-juegos/fortnite-launcher-web/downloads/version.json";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            var json = await Http.GetStringAsync(VersionInfoUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var remoteVersion = root.GetProperty("version").GetString() ?? "";
            var downloadUrl = root.GetProperty("downloadUrl").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(remoteVersion) || string.IsNullOrWhiteSpace(downloadUrl))
                return null;

            if (Version.TryParse(remoteVersion, out var remote) &&
                Version.TryParse(CurrentVersion, out var current) &&
                remote > current)
            {
                return new UpdateInfo(remoteVersion, downloadUrl);
            }
        }
        catch
        {
            // Sin conexion o version.json no disponible: se ignora.
        }

        return null;
    }

    /// <summary>
    /// Descarga la actualizacion y lanza un script que espera a que este
    /// proceso se cierre, copia los archivos nuevos, y vuelve a abrir la app.
    /// El llamador es responsable de cerrar la aplicacion (Application.Exit)
    /// justo despues de invocar esto.
    /// </summary>
    public static async Task<bool> PrepareUpdateAsync(UpdateInfo info)
    {
        try
        {
            var updateRoot = Path.Combine(Path.GetTempPath(), "FortniteLauncherUpdate");
            if (Directory.Exists(updateRoot)) Directory.Delete(updateRoot, true);
            Directory.CreateDirectory(updateRoot);

            var zipPath = Path.Combine(updateRoot, "update.zip");
            await using (var stream = await Http.GetStreamAsync(info.DownloadUrl))
            await using (var file = File.Create(zipPath))
            {
                await stream.CopyToAsync(file);
            }

            var extractPath = Path.Combine(updateRoot, "extracted");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            var installDir = AppContext.BaseDirectory.TrimEnd('\\');
            var exeName = Path.GetFileName(Environment.ProcessPath ?? "FortniteLauncher.exe");

            var scriptPath = Path.Combine(updateRoot, "apply_update.bat");
            var script =
                "@echo off\r\n" +
                $"robocopy \"{extractPath}\" \"{installDir}\" /E /R:15 /W:1 /NFL /NDL /NJH /NJS\r\n" +
                $"start \"\" \"{installDir}\\{exeName}\"\r\n";
            await File.WriteAllTextAsync(scriptPath, script);

            var psi = new ProcessStartInfo("cmd.exe", $"/c \"{scriptPath}\"")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            Process.Start(psi);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
