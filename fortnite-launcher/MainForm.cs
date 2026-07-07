using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FortniteLauncher;

public class MainForm : Form
{
    private static readonly Color BgDark = Color.FromArgb(15, 15, 17);
    private static readonly Color SidebarBg = Color.FromArgb(10, 10, 12);
    private static readonly Color NavHover = Color.FromArgb(35, 35, 40);
    private static readonly Color NavSelected = Color.FromArgb(45, 45, 52);
    private static readonly Color CardBg = Color.FromArgb(24, 24, 27);
    private static readonly Color BorderColor = Color.FromArgb(55, 55, 60);
    private static readonly Color TextSecondary = Color.FromArgb(170, 170, 175);

    private readonly Button _navPerf;
    private readonly Button _navControllers;
    private readonly Button _navPromo;
    private readonly Panel _perfPanel;
    private readonly Panel _controllersPanel;
    private readonly Panel _promoPanel;

    private readonly Label _statusLabel;
    private readonly Button _playButton;
    private readonly ListBox _controllerList;
    private readonly System.Windows.Forms.Timer _controllerTimer;

    private readonly CheckBox _chkPriority;
    private readonly CheckBox _chkPowerPlan;
    private readonly CheckBox _chkCloseApps;
    private readonly CheckBox _chkTrimMemory;
    private readonly CheckedListBox _heavyAppsList;
    private readonly RadioButton _radRendimiento;
    private readonly RadioButton _radCalidad;
    private readonly RadioButton _radSinCambios;
    private readonly Label _graphicsStatusLabel;
    private readonly Label _gameModeStatusLabel;
    private readonly Button _gameModeButton;
    private readonly Label _gameDvrStatusLabel;
    private readonly Button _gameDvrButton;

    private readonly FlowLayoutPanel _promotionsPanel;
    private readonly Label _promotionsStatusLabel;

    private EpicGameInfo? _fortnite;

    public MainForm()
    {
        Text = "Fortnite Launcher Ligero";
        ClientSize = new Size(580, 560);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgDark;

        // --- Sidebar ---
        var sidebar = new Panel { Location = new Point(0, 0), Size = new Size(150, 560), BackColor = SidebarBg };

        var logo = new Label
        {
            Text = "FORTNITE",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
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
            Location = new Point(18, 46)
        };

        _navPerf = CreateNavButton("Rendimiento", 90);
        _navControllers = CreateNavButton("Mandos", 132);
        _navPromo = CreateNavButton("Promociones", 174);

        sidebar.Controls.Add(logo);
        sidebar.Controls.Add(subtitle);
        sidebar.Controls.Add(_navPerf);
        sidebar.Controls.Add(_navControllers);
        sidebar.Controls.Add(_navPromo);

        _statusLabel = new Label
        {
            Text = "Buscando instalacion de Fortnite...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(166, 18),
            MaximumSize = new Size(400, 0)
        };

        var contentLocation = new Point(166, 55);
        var contentSize = new Size(400, 445);

        _perfPanel = new Panel { Location = contentLocation, Size = contentSize, BackColor = BgDark, Visible = true };
        _controllersPanel = new Panel { Location = contentLocation, Size = contentSize, BackColor = BgDark, Visible = false };
        _promoPanel = new Panel { Location = contentLocation, Size = contentSize, BackColor = BgDark, Visible = false };

        // --- Panel Rendimiento ---
        _chkPriority = CreateCheckBox("Subir prioridad del proceso de Fortnite", 0, true);
        _chkPowerPlan = CreateCheckBox("Usar plan de energia Maximo rendimiento mientras jugás", 25, true);
        _chkTrimMemory = CreateCheckBox("Liberar RAM de otros procesos antes de jugar", 50, true);
        _chkCloseApps = CreateCheckBox("Cerrar apps pesadas en segundo plano al jugar:", 75, true);

        _heavyAppsList = new CheckedListBox
        {
            Location = new Point(16, 100),
            Size = new Size(306, 80),
            CheckOnClick = true,
            BackColor = CardBg,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var refreshAppsButton = CreateFlatButton("Actualizar", new Point(328, 100), new Size(72, 26));
        refreshAppsButton.Click += (_, _) => RefreshHeavyAppsList();

        var graphicsLabel = new Label
        {
            Text = "Calidad grafica al jugar:",
            ForeColor = Color.White,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 195)
        };

        _radRendimiento = CreateRadio("Rendimiento (recomendado)", 217, true);
        _radCalidad = CreateRadio("Calidad", 239, false);
        _radSinCambios = CreateRadio("No cambiar nada", 261, false);

        _graphicsStatusLabel = new Label
        {
            Text = "",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 285),
            MaximumSize = new Size(390, 0)
        };

        _gameModeStatusLabel = new Label
        {
            Text = "Modo Juego de Windows: consultando...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 320)
        };

        _gameModeButton = CreateFlatButton("Activar/Desactivar", new Point(0, 344), new Size(180, 28));
        _gameModeButton.Click += OnToggleGameMode;

        _gameDvrStatusLabel = new Label
        {
            Text = "Grabacion en segundo plano (Game DVR): consultando...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 380)
        };

        _gameDvrButton = CreateFlatButton("Activar/Desactivar", new Point(0, 404), new Size(180, 28));
        _gameDvrButton.Click += OnToggleGameDvr;

        _perfPanel.Controls.AddRange(new Control[]
        {
            _chkPriority, _chkPowerPlan, _chkTrimMemory, _chkCloseApps, _heavyAppsList, refreshAppsButton,
            graphicsLabel, _radRendimiento, _radCalidad, _radSinCambios, _graphicsStatusLabel,
            _gameModeStatusLabel, _gameModeButton, _gameDvrStatusLabel, _gameDvrButton
        });

        // --- Panel Mandos ---
        _controllerList = new ListBox
        {
            Location = new Point(0, 0),
            Size = new Size(400, 445),
            BackColor = CardBg,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        _controllersPanel.Controls.Add(_controllerList);

        // --- Panel Promociones ---
        _promotionsStatusLabel = new Label
        {
            Text = "Cargando promociones de Epic Games...",
            ForeColor = TextSecondary,
            BackColor = BgDark,
            AutoSize = true,
            Location = new Point(0, 4)
        };

        var refreshPromotionsButton = CreateFlatButton("Actualizar", new Point(322, 0), new Size(78, 26));
        refreshPromotionsButton.Click += (_, _) => _ = RefreshPromotionsAsync();

        _promotionsPanel = new FlowLayoutPanel
        {
            Location = new Point(0, 32),
            Size = new Size(400, 413),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = BgDark
        };

        _promoPanel.Controls.Add(_promotionsStatusLabel);
        _promoPanel.Controls.Add(refreshPromotionsButton);
        _promoPanel.Controls.Add(_promotionsPanel);

        // --- Boton jugar ---
        _playButton = new Button
        {
            Text = "JUGAR",
            Location = new Point(166, 510),
            Size = new Size(400, 42),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Enabled = false
        };
        _playButton.FlatAppearance.BorderSize = 0;
        _playButton.Click += OnPlayClicked;

        Controls.Add(sidebar);
        Controls.Add(_statusLabel);
        Controls.Add(_perfPanel);
        Controls.Add(_controllersPanel);
        Controls.Add(_promoPanel);
        Controls.Add(_playButton);

        _navPerf.Click += (_, _) => ShowPanel(_perfPanel, _navPerf);
        _navControllers.Click += (_, _) => ShowPanel(_controllersPanel, _navControllers);
        _navPromo.Click += (_, _) => ShowPanel(_promoPanel, _navPromo);
        ShowPanel(_perfPanel, _navPerf);

        _controllerTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _controllerTimer.Tick += (_, _) => RefreshControllers();
        _controllerTimer.Start();

        Load += (_, _) =>
        {
            DetectFortnite();
            RefreshHeavyAppsList();
            RefreshGameModeStatus();
            RefreshGameDvrStatus();
            _ = RefreshPromotionsAsync();
        };
    }

    private Button CreateNavButton(string text, int y)
    {
        var button = new Button
        {
            Text = "   " + text,
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            BackColor = SidebarBg,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(0, y),
            Size = new Size(150, 36),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = NavHover;
        return button;
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

    private static RadioButton CreateRadio(string text, int y, bool defaultChecked) => new()
    {
        Text = text,
        Checked = defaultChecked,
        AutoSize = true,
        ForeColor = Color.White,
        BackColor = BgDark,
        Location = new Point(16, y)
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

    private void ShowPanel(Panel panelToShow, Button navButton)
    {
        _perfPanel.Visible = panelToShow == _perfPanel;
        _controllersPanel.Visible = panelToShow == _controllersPanel;
        _promoPanel.Visible = panelToShow == _promoPanel;

        foreach (var nav in new[] { _navPerf, _navControllers, _navPromo })
        {
            nav.BackColor = SidebarBg;
        }
        navButton.BackColor = NavSelected;
    }

    private void DetectFortnite()
    {
        if (!EpicGameFinder.IsEpicGamesLauncherInstalled())
        {
            _statusLabel.Text = "No se encontro Epic Games Launcher instalado.";
            return;
        }

        _fortnite = EpicGameFinder.FindFortnite();
        if (_fortnite is null)
        {
            _statusLabel.Text = "Epic Games Launcher encontrado, pero Fortnite no parece estar instalado.";
            return;
        }

        _statusLabel.Text = $"Fortnite detectado en: {_fortnite.InstallLocation}";
        _playButton.Enabled = true;
    }

    private void RefreshControllers()
    {
        var controllers = ControllerMonitor.GetConnectedControllers();

        _controllerList.Items.Clear();
        if (controllers.Count == 0)
        {
            _controllerList.Items.Add("No se detectan mandos conectados.");
            return;
        }

        foreach (var c in controllers)
        {
            var suffix = c.LikelyCompatible ? "" : "  (puede necesitar DS4Windows/BetterJoy)";
            _controllerList.Items.Add(c.Name + suffix);
        }
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

    private async Task RefreshPromotionsAsync()
    {
        _promotionsStatusLabel.Text = "Cargando promociones de Epic Games...";
        _promotionsPanel.Controls.Clear();

        var promos = await EpicPromotions.FetchCurrentAndUpcomingAsync();

        if (promos.Count == 0)
        {
            _promotionsStatusLabel.Text = "No se pudieron cargar promociones (revisá tu conexión).";
            return;
        }

        _promotionsStatusLabel.Text = "Juegos gratis en Epic Games:";
        foreach (var promo in promos)
        {
            _promotionsPanel.Controls.Add(BuildPromoCard(promo));
        }
    }

    private static Panel BuildPromoCard(PromoGame promo)
    {
        var card = new Panel
        {
            Size = new Size(385, 64),
            Margin = new Padding(0, 0, 0, 8),
            BackColor = CardBg,
            BorderStyle = BorderStyle.FixedSingle
        };

        var titleLabel = new Label
        {
            Text = promo.Title,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = CardBg,
            AutoSize = true,
            Location = new Point(8, 6),
            MaximumSize = new Size(260, 0)
        };

        string statusText;
        if (promo.IsCurrentlyFree)
        {
            statusText = promo.FreeUntil is { } until
                ? $"Gratis hasta el {until.ToLocalTime():dd/MM/yyyy}"
                : "Gratis ahora";
        }
        else
        {
            statusText = promo.FreeFrom is { } from
                ? $"Gratis desde el {from.ToLocalTime():dd/MM/yyyy}"
                : "Proximamente gratis";
        }

        var statusLabel = new Label
        {
            Text = statusText,
            ForeColor = TextSecondary,
            BackColor = CardBg,
            AutoSize = true,
            Location = new Point(8, 28)
        };

        var viewButton = CreateFlatButton("Ver en tienda", new Point(275, 18), new Size(100, 26));
        viewButton.Enabled = promo.StoreUrl is not null;
        viewButton.Click += (_, _) =>
        {
            if (promo.StoreUrl is null) return;
            try
            {
                Process.Start(new ProcessStartInfo(promo.StoreUrl) { UseShellExecute = true });
            }
            catch
            {
                // Si no hay navegador asociado, se ignora.
            }
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(statusLabel);
        card.Controls.Add(viewButton);
        return card;
    }

    private void OnPlayClicked(object? sender, EventArgs e)
    {
        if (_fortnite is null) return;

        if (!_radSinCambios.Checked)
        {
            var profile = _radRendimiento.Checked ? GraphicsQualityProfile.Rendimiento : GraphicsQualityProfile.Calidad;
            if (GraphicsProfileManager.IsFortniteRunning())
            {
                _graphicsStatusLabel.Text = "Cerra Fortnite y volve a abrirlo para aplicar el cambio de calidad.";
            }
            else
            {
                var applied = GraphicsProfileManager.Apply(profile);
                _graphicsStatusLabel.Text = applied
                    ? "Perfil de calidad grafica aplicado."
                    : "No se pudo aplicar el perfil (¿nunca abriste Fortnite antes?).";
            }
        }

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

        string? originalScheme = null;
        if (_chkPowerPlan.Checked)
        {
            originalScheme = PerformanceTools.GetActiveSchemeGuid();
            PerformanceTools.EnableMaxPerformancePlan();
        }

        try
        {
            var uri = _fortnite.BuildLaunchUri();
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo iniciar Fortnite: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            PerformanceTools.RestoreScheme(originalScheme);
            return;
        }

        if (_chkPriority.Checked || _chkPowerPlan.Checked)
        {
            _ = MonitorFortniteSessionAsync(_chkPriority.Checked, _chkPowerPlan.Checked, originalScheme);
        }
    }

    private static async Task MonitorFortniteSessionAsync(bool boostPriority, bool restorePowerPlanOnExit, string? originalSchemeGuid)
    {
        const string processName = "FortniteClient-Win64-Shipping";
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
                // Si no se puede escuchar el cierre, el plan queda en Alto rendimiento
                // hasta que se restaure manualmente con powercfg.
            }
        }
    }
}
