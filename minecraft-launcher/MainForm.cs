using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftLauncher;

public class MainForm : Form
{
    private static readonly Color BgDark = Color.FromArgb(15, 15, 17);
    private static readonly Color SidebarBg = Color.FromArgb(10, 10, 12);
    private static readonly Color NavHover = Color.FromArgb(35, 35, 40);
    private static readonly Color CardBg = Color.FromArgb(24, 24, 27);
    private static readonly Color BorderColor = Color.FromArgb(55, 55, 60);
    private static readonly Color TextSecondary = Color.FromArgb(170, 170, 175);

    private readonly Label _javaStatusLabel;
    private readonly Button _javaButton;
    private readonly Label _bedrockStatusLabel;
    private readonly Button _bedrockButton;

    private readonly CheckBox _chkPriority;
    private readonly CheckBox _chkPowerPlan;
    private readonly CheckBox _chkCloseApps;
    private readonly CheckBox _chkTrimMemory;
    private readonly CheckedListBox _heavyAppsList;
    private readonly Label _gameModeStatusLabel;
    private readonly Button _gameModeButton;
    private readonly Label _gameDvrStatusLabel;
    private readonly Button _gameDvrButton;
    private readonly CheckBox _chkEmptyRecycleBin;
    private readonly Button _cleanupButton;
    private readonly Label _cleanupStatusLabel;

    private readonly Button _fullscreenButton;

    private bool _isFullScreen;
    private FormWindowState _restoreWindowState;
    private FormBorderStyle _restoreBorderStyle;
    private Rectangle _restoreBounds;

    public MainForm()
    {
        Text = "Minecraft Launcher Ligero";
        ClientSize = new Size(580, 560);
        MinimumSize = new Size(580, 560);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgDark;
        KeyPreview = true;
        KeyDown += OnMainFormKeyDown;

        // --- Sidebar ---
        var sidebar = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(150, 560),
            BackColor = SidebarBg,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
        };

        var logo = new Label
        {
            Text = "MINECRAFT",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = SidebarBg,
            AutoSize = true,
            Location = new Point(18, 20)
        };

        var subtitle = new Label
        {
            Text = "Launcher Ligero",
            Font = new Font("Segoe UI", 8),
            ForeColor = TextSecondary,
            BackColor = SidebarBg,
            AutoSize = true,
            Location = new Point(18, 44)
        };

        _fullscreenButton = CreateFlatButton("Pantalla completa", new Point(10, 514), new Size(130, 30));
        _fullscreenButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _fullscreenButton.Click += (_, _) => ToggleFullScreen();

        sidebar.Controls.Add(logo);
        sidebar.Controls.Add(subtitle);
        sidebar.Controls.Add(_fullscreenButton);

        // --- Contenido ---
        var content = new Panel
        {
            Location = new Point(166, 18),
            Size = new Size(400, 522),
            BackColor = BgDark,
            AutoScroll = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        var javaTitle = new Label
        {
            Text = "Java Edition",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        _javaStatusLabel = new Label
        {
            Text = "Buscando instalacion...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 22),
            MaximumSize = new Size(390, 0)
        };

        _javaButton = new Button
        {
            Text = "JUGAR",
            Location = new Point(0, 48),
            Size = new Size(390, 40),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Enabled = false
        };
        _javaButton.FlatAppearance.BorderSize = 0;
        _javaButton.Click += OnJavaButtonClicked;

        var bedrockTitle = new Label
        {
            Text = "Bedrock Edition",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 104)
        };

        _bedrockStatusLabel = new Label
        {
            Text = "Buscando instalacion...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 126),
            MaximumSize = new Size(390, 0)
        };

        _bedrockButton = new Button
        {
            Text = "JUGAR",
            Location = new Point(0, 152),
            Size = new Size(390, 40),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Enabled = false
        };
        _bedrockButton.FlatAppearance.BorderSize = 0;
        _bedrockButton.Click += OnBedrockButtonClicked;

        var perfDivider = new Label
        {
            Text = "Rendimiento",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 212)
        };

        _chkPriority = CreateCheckBox("Subir prioridad del proceso (solo Bedrock)", 236, true);
        _chkPowerPlan = CreateCheckBox("Usar plan de energia Maximo rendimiento", 258, true);
        _chkTrimMemory = CreateCheckBox("Liberar RAM de otros procesos antes de jugar", 280, true);
        _chkCloseApps = CreateCheckBox("Cerrar apps pesadas en segundo plano al jugar:", 302, true);

        _heavyAppsList = new CheckedListBox
        {
            Location = new Point(16, 326),
            Size = new Size(306, 74),
            CheckOnClick = true,
            BackColor = CardBg,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var refreshAppsButton = CreateFlatButton("Actualizar", new Point(328, 326), new Size(72, 26));
        refreshAppsButton.Click += (_, _) => RefreshHeavyAppsList();

        _gameModeStatusLabel = new Label
        {
            Text = "Modo Juego de Windows: consultando...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 414)
        };

        _gameModeButton = CreateFlatButton("Activar/Desactivar", new Point(0, 438), new Size(180, 28));
        _gameModeButton.Click += OnToggleGameMode;

        _gameDvrStatusLabel = new Label
        {
            Text = "Grabacion en segundo plano (Game DVR): consultando...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 474)
        };

        _gameDvrButton = CreateFlatButton("Activar/Desactivar", new Point(0, 498), new Size(180, 28));
        _gameDvrButton.Click += OnToggleGameDvr;

        var cleanupLabel = new Label
        {
            Text = "Limpieza de Windows:",
            ForeColor = Color.White,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 534)
        };

        _chkEmptyRecycleBin = CreateCheckBox("Vaciar tambien la papelera de reciclaje", 556, false);

        _cleanupButton = CreateFlatButton("Limpiar archivos temporales", new Point(0, 582), new Size(200, 28));
        _cleanupButton.Click += OnCleanupClicked;

        _cleanupStatusLabel = new Label
        {
            Text = "",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 616),
            MaximumSize = new Size(380, 0)
        };

        content.Controls.AddRange(new Control[]
        {
            javaTitle, _javaStatusLabel, _javaButton,
            bedrockTitle, _bedrockStatusLabel, _bedrockButton,
            perfDivider, _chkPriority, _chkPowerPlan, _chkTrimMemory, _chkCloseApps,
            _heavyAppsList, refreshAppsButton,
            _gameModeStatusLabel, _gameModeButton, _gameDvrStatusLabel, _gameDvrButton,
            cleanupLabel, _chkEmptyRecycleBin, _cleanupButton, _cleanupStatusLabel
        });

        Controls.Add(sidebar);
        Controls.Add(content);

        Load += (_, _) =>
        {
            RefreshDetection();
            RefreshHeavyAppsList();
            RefreshGameModeStatus();
            RefreshGameDvrStatus();
            ToggleFullScreen();
        };
    }

    private static CheckBox CreateCheckBox(string text, int y, bool defaultChecked) => new()
    {
        Text = text,
        Checked = defaultChecked,
        AutoSize = true,
        ForeColor = Color.White,
        BackColor = BgDark,
        Location = new Point(0, y)
    };

    private static Button CreateFlatButton(string text, Point location, Size size)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = CardBg,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8.5f)
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = BorderColor;
        button.FlatAppearance.MouseOverBackColor = NavHover;
        return button;
    }

    private void OnMainFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F11)
        {
            ToggleFullScreen();
        }
        else if (e.KeyCode == Keys.Escape && _isFullScreen)
        {
            ToggleFullScreen();
        }
    }

    private void ToggleFullScreen()
    {
        if (!_isFullScreen)
        {
            _restoreBorderStyle = FormBorderStyle;
            _restoreWindowState = WindowState;
            _restoreBounds = Bounds;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            _isFullScreen = true;
            _fullscreenButton.Text = "Salir (Esc)";
        }
        else
        {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = _restoreBorderStyle;
            WindowState = _restoreWindowState;
            Bounds = _restoreBounds;
            _isFullScreen = false;
            _fullscreenButton.Text = "Pantalla completa";
        }
    }

    private void RefreshDetection()
    {
        if (MinecraftGameFinder.IsJavaInstalled())
        {
            _javaStatusLabel.Text = "Instalado.";
            _javaButton.Text = "JUGAR";
        }
        else
        {
            _javaStatusLabel.Text = "No esta instalado. Te lleva a descargarlo desde minecraft.net.";
            _javaButton.Text = "INSTALAR (minecraft.net)";
        }
        _javaButton.Enabled = true;

        if (MinecraftGameFinder.IsBedrockInstalled())
        {
            _bedrockStatusLabel.Text = "Instalado (Microsoft Store).";
            _bedrockButton.Text = "JUGAR";
        }
        else
        {
            _bedrockStatusLabel.Text = "No esta instalado. Te lleva a instalarlo desde Microsoft Store.";
            _bedrockButton.Text = "INSTALAR (Microsoft Store)";
        }
        _bedrockButton.Enabled = true;
    }

    private void RefreshHeavyAppsList()
    {
        var running = PerformanceTools.GetRunningHeavyProcessNames();
        _heavyAppsList.Items.Clear();
        foreach (var name in running)
        {
            _heavyAppsList.Items.Add(name, true);
        }
        if (running.Count == 0)
        {
            _heavyAppsList.Items.Add("No hay apps pesadas conocidas corriendo.", false);
        }
    }

    private void RefreshGameModeStatus()
    {
        var enabled = PerformanceTools.IsWindowsGameModeEnabled();
        _gameModeStatusLabel.Text = $"Modo Juego de Windows: {(enabled ? "activado" : "desactivado")}";
    }

    private void OnToggleGameMode(object? sender, EventArgs e)
    {
        var newState = !PerformanceTools.IsWindowsGameModeEnabled();
        PerformanceTools.SetWindowsGameModeEnabled(newState);
        RefreshGameModeStatus();
    }

    private void RefreshGameDvrStatus()
    {
        var enabled = PerformanceTools.IsGameDvrEnabled();
        _gameDvrStatusLabel.Text = $"Grabacion en segundo plano (Game DVR): {(enabled ? "activada" : "desactivada")}";
    }

    private void OnToggleGameDvr(object? sender, EventArgs e)
    {
        var newState = !PerformanceTools.IsGameDvrEnabled();
        PerformanceTools.SetGameDvrEnabled(newState);
        RefreshGameDvrStatus();
    }

    private async void OnCleanupClicked(object? sender, EventArgs e)
    {
        var includeRecycleBin = _chkEmptyRecycleBin.Checked;

        var confirm = MessageBox.Show(
            includeRecycleBin
                ? "Se van a borrar los archivos temporales de Windows y se va a vaciar la papelera de reciclaje. No se puede deshacer. ¿Continuar?"
                : "Se van a borrar los archivos temporales de Windows. No se puede deshacer. ¿Continuar?",
            "Limpieza de Windows",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.OK) return;

        _cleanupButton.Enabled = false;
        _cleanupStatusLabel.Text = "Limpiando...";

        var result = await Task.Run(() =>
        {
            var cleanupResult = WindowsCleanup.CleanTempFiles();
            if (includeRecycleBin)
            {
                WindowsCleanup.EmptyRecycleBin();
            }
            return cleanupResult;
        });

        _cleanupButton.Enabled = true;
        var freedMb = result.BytesFreed / 1024.0 / 1024.0;
        var skippedNote = result.FilesSkipped > 0 ? $" ({result.FilesSkipped} en uso, omitidos)" : "";
        _cleanupStatusLabel.Text = $"Listo: {result.FilesDeleted} archivos borrados, {freedMb:0.#} MB liberados{skippedNote}";
    }

    private void ApplyPreLaunchTools()
    {
        if (_chkCloseApps.Checked)
        {
            var selected = _heavyAppsList.CheckedItems.Cast<object>()
                .Select(i => i.ToString() ?? "")
                .Where(n => PerformanceTools.KnownHeavyProcessNames.Contains(n))
                .ToList();

            if (selected.Count > 0)
            {
                var confirm = MessageBox.Show(
                    $"Se van a cerrar estas apps antes de jugar:\n\n{string.Join("\n", selected)}\n\n¿Continuar?",
                    "Confirmar cierre de apps",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (confirm == DialogResult.OK)
                {
                    PerformanceTools.CloseProcesses(selected);
                }
            }
        }

        if (_chkTrimMemory.Checked)
        {
            _ = Task.Run(() => PerformanceTools.TrimBackgroundProcesses());
        }
    }

    private void OnJavaButtonClicked(object? sender, EventArgs e)
    {
        if (!MinecraftGameFinder.IsJavaInstalled())
        {
            MinecraftGameFinder.OpenJavaDownloadPage();
            return;
        }

        ApplyPreLaunchTools();

        if (_chkPowerPlan.Checked)
        {
            PerformanceTools.EnableMaxPerformancePlan();
        }

        MinecraftGameFinder.LaunchJava();

        // El launcher de Java corre en javaw.exe (nombre generico, compartido
        // por otras apps Java), asi que no es seguro identificar ni subirle
        // la prioridad, ni restaurar el plan de energia automaticamente al
        // cerrar. Se avisa en pantalla en vez de arriesgar tocar el proceso
        // equivocado.
        if (_chkPowerPlan.Checked)
        {
            _javaStatusLabel.Text = "Jugando con plan Maximo rendimiento activo (se mantiene hasta que lo cambies vos).";
        }
    }

    private void OnBedrockButtonClicked(object? sender, EventArgs e)
    {
        if (!MinecraftGameFinder.IsBedrockInstalled())
        {
            MinecraftGameFinder.OpenBedrockDownloadPage();
            return;
        }

        ApplyPreLaunchTools();

        string? originalScheme = null;
        if (_chkPowerPlan.Checked)
        {
            originalScheme = PerformanceTools.GetActiveSchemeGuid();
            PerformanceTools.EnableMaxPerformancePlan();
        }

        MinecraftGameFinder.LaunchBedrock();

        if (_chkPriority.Checked || _chkPowerPlan.Checked)
        {
            _ = MonitorBedrockSessionAsync(_chkPriority.Checked, _chkPowerPlan.Checked, originalScheme);
        }
    }

    private static async Task MonitorBedrockSessionAsync(bool boostPriority, bool restorePowerPlanOnExit, string? originalSchemeGuid)
    {
        const string processName = "Minecraft.Windows";
        var deadline = DateTime.UtcNow.AddMinutes(3);
        Process? target = null;

        while (DateTime.UtcNow < deadline)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                target = processes[0];
                break;
            }
            await Task.Delay(2000);
        }

        if (target is null)
        {
            if (restorePowerPlanOnExit) PerformanceTools.RestoreScheme(originalSchemeGuid);
            return;
        }

        if (boostPriority)
        {
            try { target.PriorityClass = ProcessPriorityClass.High; }
            catch { /* puede requerir permisos elevados */ }
        }

        if (restorePowerPlanOnExit)
        {
            try
            {
                target.EnableRaisingEvents = true;
                target.Exited += (_, _) => PerformanceTools.RestoreScheme(originalSchemeGuid);
            }
            catch
            {
                // Si no se puede escuchar el cierre, el plan queda activo
                // hasta que se restaure manualmente con powercfg.
            }
        }
    }
}
