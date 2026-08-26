using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MinecraftLauncher;

public record CleanupResult(long BytesFreed, int FilesDeleted, int FilesSkipped);

/// <summary>
/// Limpieza de archivos temporales y papelera de reciclaje. Solo toca
/// carpetas de temporales conocidas (no archivos del sistema ni del juego);
/// cualquier archivo bloqueado o sin permisos se omite en silencio.
/// </summary>
public static class WindowsCleanup
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private const uint SherbNoConfirmation = 0x00000001;
    private const uint SherbNoProgressUi = 0x00000002;
    private const uint SherbNoSound = 0x00000004;

    public static CleanupResult CleanTempFiles()
    {
        long bytesFreed = 0;
        var deleted = 0;
        var skipped = 0;

        var folders = new[]
        {
            Path.GetTempPath(),
            Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp")
        };

        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                CleanFolderRecursive(folder, ref bytesFreed, ref deleted, ref skipped);
            }
        }

        return new CleanupResult(bytesFreed, deleted, skipped);
    }

    public static void EmptyRecycleBin()
    {
        try
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, SherbNoConfirmation | SherbNoProgressUi | SherbNoSound);
        }
        catch
        {
            // Si falla (por ejemplo, ya esta vacia), se ignora.
        }
    }

    private static void CleanFolderRecursive(string folder, ref long bytesFreed, ref int deleted, ref int skipped)
    {
        string[] files;
        try { files = Directory.GetFiles(folder); }
        catch { return; }

        foreach (var file in files)
        {
            try
            {
                var info = new FileInfo(file);
                var size = info.Length;
                info.Delete();
                bytesFreed += size;
                deleted++;
            }
            catch
            {
                skipped++;
            }
        }

        string[] subDirs;
        try { subDirs = Directory.GetDirectories(folder); }
        catch { return; }

        foreach (var dir in subDirs)
        {
            CleanFolderRecursive(dir, ref bytesFreed, ref deleted, ref skipped);
            try
            {
                if (Directory.GetFileSystemEntries(dir).Length == 0)
                {
                    Directory.Delete(dir);
                }
            }
            catch
            {
                // Carpeta en uso o no vacia: se ignora.
            }
        }
    }
}
