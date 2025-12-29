using HASS.Agent.Models.Internal;
using HASS.Agent.Resources.Localization;
using Serilog;
using Syncfusion.Windows.Forms;
using System.Drawing;

namespace HASS.Agent.Managers
{
    /// <summary>
    /// Manages the system tray icon functionality
    /// </summary>
    public class TrayIconManager
    {
        private readonly Main _mainForm;
        private readonly NotifyIcon _notifyIcon;
        private ContextMenuStrip _trayMenu;

        public TrayIconManager(Main mainForm, NotifyIcon notifyIcon)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
        }

        /// <summary>
        /// Initializes the tray icon with menu items
        /// </summary>
        public void Initialize()
        {
            try
            {
                _notifyIcon.MouseClick += NotifyIcon_MouseClick;
                _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

                BuildTrayMenu();

                Log.Information("[TRAYICON] Tray icon initialized");
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error initializing tray icon: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Builds the context menu for the tray icon
        /// </summary>
        private void BuildTrayMenu()
        {
            try
            {
                _trayMenu = new ContextMenuStrip();

                // Show
                var showItem = new ToolStripMenuItem(Languages.TrayIcon_Show);
                showItem.Click += (s, e) => _mainForm.ShowMain();
                _trayMenu.Items.Add(showItem);

                // Quick Actions
                var qaItem = new ToolStripMenuItem(Languages.TrayIcon_QuickActions);
                qaItem.Click += (s, e) => _mainForm.ShowQuickActions();
                _trayMenu.Items.Add(qaItem);

                // Configuration
                var configItem = new ToolStripMenuItem(Languages.TrayIcon_Configuration);
                configItem.Click += (s, e) => _mainForm.ShowConfiguration();
                _trayMenu.Items.Add(configItem);

                _trayMenu.Items.Add(new ToolStripSeparator());

                // Sensors
                var sensorsItem = new ToolStripMenuItem(Languages.TrayIcon_Sensors);
                sensorsItem.Click += (s, e) => _mainForm.ShowSensorsManager();
                _trayMenu.Items.Add(sensorsItem);

                // Commands
                var commandsItem = new ToolStripMenuItem(Languages.TrayIcon_Commands);
                commandsItem.Click += (s, e) => _mainForm.ShowCommandsManager();
                _trayMenu.Items.Add(commandsItem);

                // Quick Actions Config
                var qaConfigItem = new ToolStripMenuItem(Languages.TrayIcon_QuickActionsConfig);
                qaConfigItem.Click += (s, e) => _mainForm.ShowQuickActionsManager();
                _trayMenu.Items.Add(qaConfigItem);

                // Satellite Service
                var serviceItem = new ToolStripMenuItem(Languages.TrayIcon_SatelliteService);
                serviceItem.Click += (s, e) => _mainForm.ShowServiceManager();
                _trayMenu.Items.Add(serviceItem);

                _trayMenu.Items.Add(new ToolStripSeparator());

                // Check for Updates
                var updateItem = new ToolStripMenuItem(Languages.TrayIcon_CheckUpdates);
                updateItem.Click += async (s, e) => await _mainForm.CheckForUpdate();
                _trayMenu.Items.Add(updateItem);

                // Help
                var helpItem = new ToolStripMenuItem(Languages.TrayIcon_Help);
                helpItem.Click += (s, e) => _mainForm.BtnHelp_Click(s, e);
                _trayMenu.Items.Add(helpItem);

                // About
                var aboutItem = new ToolStripMenuItem(Languages.TrayIcon_About);
                aboutItem.Click += (s, e) => _mainForm.TsAbout_Click(s, e);
                _trayMenu.Items.Add(aboutItem);

                _trayMenu.Items.Add(new ToolStripSeparator());

                // Exit
                var exitItem = new ToolStripMenuItem(Languages.TrayIcon_Exit);
                exitItem.Click += (s, e) => _mainForm.Exit();
                _trayMenu.Items.Add(exitItem);

                _notifyIcon.ContextMenuStrip = _trayMenu;

                Log.Debug("[TRAYICON] Tray menu built successfully");
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error building tray menu: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Shows the tray icon
        /// </summary>
        public void Show()
        {
            try
            {
                _notifyIcon.Visible = true;
                Log.Debug("[TRAYICON] Tray icon shown");
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error showing tray icon: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Hides the tray icon
        /// </summary>
        public void Hide()
        {
            try
            {
                _notifyIcon.Visible = false;
                Log.Debug("[TRAYICON] Tray icon hidden");
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error hiding tray icon: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Shows a balloon tip notification
        /// </summary>
        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 3000)
        {
            try
            {
                _notifyIcon.BalloonTipTitle = title;
                _notifyIcon.BalloonTipText = message;
                _notifyIcon.BalloonTipIcon = icon;
                _notifyIcon.ShowBalloonTip(timeout);

                Log.Debug("[TRAYICON] Notification shown: {Title}", title);
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error showing notification: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Updates the tray icon text
        /// </summary>
        public void UpdateText(string text)
        {
            try
            {
                _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error updating tray text: {Err}", ex.Message);
            }
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    _mainForm.ShowQuickActions();
                }
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error handling mouse click: {Err}", ex.Message);
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                _mainForm.ShowMain();
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error handling double click: {Err}", ex.Message);
            }
        }

        /// <summary>
        /// Cleans up resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _notifyIcon?.Dispose();
                _trayMenu?.Dispose();

                Log.Debug("[TRAYICON] Tray icon manager disposed");
            }
            catch (Exception ex)
            {
                Log.Error("[TRAYICON] Error disposing tray icon manager: {Err}", ex.Message);
            }
        }
    }
}
