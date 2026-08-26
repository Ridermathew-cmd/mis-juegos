using System;
using System.Diagnostics;
using System.IO;

namespace MinecraftLauncher;

/// <summary>
/// Detecta y lanza Minecraft Java Edition y/o Bedrock Edition. No instala
/// nada por si mismo: si falta una edicion, lleva a su canal oficial de
/// instalacion (minecraft.net para Java, Microsoft Store para Bedrock).
/// </summary>
public static class MinecraftGameFinder
{
    private const string JavaLauncherPathX86 = @"C:\Program Files (x86)\Minecraft Launcher\MinecraftLauncher.exe";
    private const string JavaLauncherPath64 = @"C:\Program Files\Minecraft Launcher\MinecraftLauncher.exe";
    private const string JavaStorePackageFamily = "Microsoft.4297127D64EC6_8wekyb3d8bbwe";
    private const string BedrockPackageFamily = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    public static string? FindJavaLauncherExe()
    {
        foreach (var path in new[] { JavaLauncherPathX86, JavaLauncherPath64 })
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    public static bool IsJavaStoreAppInstalled() => IsStorePackageInstalled(JavaStorePackageFamily);

    public static bool IsBedrockInstalled() => IsStorePackageInstalled(BedrockPackageFamily);

    public static bool IsJavaInstalled() => FindJavaLauncherExe() is not null || IsJavaStoreAppInstalled();

    private static bool IsStorePackageInstalled(string packageFamilyName)
    {
        var path = Environment.ExpandEnvironmentVariables($@"%LOCALAPPDATA%\Packages\{packageFamilyName}");
        return Directory.Exists(path);
    }

    public static void LaunchJava()
    {
        var exePath = FindJavaLauncherExe();
        if (exePath is not null)
        {
            Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
            return;
        }

        if (IsJavaStoreAppInstalled())
        {
            LaunchStoreApp(JavaStorePackageFamily);
        }
    }

    public static void LaunchBedrock() => LaunchStoreApp(BedrockPackageFamily);

    private static void LaunchStoreApp(string packageFamilyName)
    {
        var psi = new ProcessStartInfo("explorer.exe", $"shell:AppsFolder\\{packageFamilyName}!App")
        {
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    public static void OpenJavaDownloadPage()
    {
        Process.Start(new ProcessStartInfo("https://www.minecraft.net/en-us/download") { UseShellExecute = true });
    }

    public static void OpenBedrockDownloadPage()
    {
        Process.Start(new ProcessStartInfo("https://www.minecraft.net/en-us/get-minecraft") { UseShellExecute = true });
    }
}
