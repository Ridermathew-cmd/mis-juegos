using System;
using System.IO;
using System.Windows.Forms;

namespace MinecraftLauncher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogCrash("AppDomain.UnhandledException", e.ExceptionObject as Exception);
        Application.ThreadException += (_, e) =>
            LogCrash("Application.ThreadException", e.Exception);

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }

    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] {source}: {ex}\n\n");
        }
        catch
        {
            // Si ni siquiera se puede escribir el log, no hay mucho mas que hacer.
        }
    }
}
