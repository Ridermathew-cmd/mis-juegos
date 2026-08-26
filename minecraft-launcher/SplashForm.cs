using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace MinecraftLauncher;

/// <summary>
/// Pantalla de bienvenida animada (pulso tipo latido) que se muestra un
/// par de segundos al abrir el launcher. Usa WebView2 (el motor de Edge,
/// incluido de fabrica en Windows 10/11 actualizados) para reproducir el
/// splash tal cual, con su animacion en Canvas. Si WebView2 no esta
/// disponible en el equipo, se cierra sola sin interrumpir el arranque:
/// es un adorno visual, no algo de lo que dependa la app para funcionar.
/// </summary>
public class SplashForm : Form
{
    // El pulso dura ~2.3s por ciclo (late fuerte y despues una pausa larga,
    // como un monitor cardiaco). Con 2.6s solo se alcanza a ver un latido
    // y despues la pausa justo antes de cerrarse, lo que da sensacion de
    // que se traba. Dejando ~2 ciclos completos se nota que es ritmico.
    private const int DisplayMilliseconds = 4800;

    private readonly WebView2 _webView;
    private readonly System.Windows.Forms.Timer _closeTimer;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = false;
        Size = new Size(800, 450);
        BackColor = Color.FromArgb(6, 10, 7);

        _webView = new WebView2 { Dock = DockStyle.Fill };
        Controls.Add(_webView);

        _closeTimer = new System.Windows.Forms.Timer { Interval = DisplayMilliseconds };
        _closeTimer.Tick += (_, _) => Close();

        Load += OnLoadAsync;
    }

    private async void OnLoadAsync(object? sender, EventArgs e)
    {
        try
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.NavigateToString(LoadSplashHtml());
            _closeTimer.Start();
        }
        catch
        {
            // WebView2 Runtime no instalado u otro problema: se omite el
            // splash directamente en vez de dejar una ventana rota.
            Close();
        }
    }

    private static string LoadSplashHtml()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("MinecraftLauncher.Assets.splash.html");
        if (stream is null) return "<body></body>";
        using var reader = new System.IO.StreamReader(stream);
        return reader.ReadToEnd();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _closeTimer.Dispose();
            _webView.Dispose();
        }
        base.Dispose(disposing);
    }
}
