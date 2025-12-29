using System.Text;
using Serilog;

namespace HASS.Agent.Forms
{
    /// <summary>
    /// 欢迎向导 - 简化的配置界面
    /// 提供三种配置方式：快速配置、高级配置、自动发现
    /// </summary>
    public partial class WelcomeWizard : Form
    {
        private enum WizardStep
        {
            Welcome,           // 欢迎页面
            QuickConfig,       // 快速配置
            Discovery,         // 自动发现
            AdvancedConfig,    // 高级配置
            Progress,          // 配置进度
            Complete           // 完成
        }

        private WizardStep _currentStep = WizardStep.Welcome;
        private readonly Dictionary<WizardStep, UserControl> _stepControls = new Dictionary<WizardStep, UserControl>();

        public WelcomeWizard()
        {
            InitializeComponent();
            InitializeSteps();
        }

        private void InitializeSteps()
        {
            // 创建各步骤的用户控件
            _stepControls[WizardStep.Welcome] = CreateWelcomeStep();
            _stepControls[WizardStep.QuickConfig] = CreateQuickConfigStep();
            _stepControls[WizardStep.Discovery] = CreateDiscoveryStep();
            _stepControls[WizardStep.AdvancedConfig] = CreateAdvancedConfigStep();
            _stepControls[WizardStep.Progress] = CreateProgressStep();
            _stepControls[WizardStep.Complete] = CreateCompleteStep();

            ShowStep(WizardStep.Welcome);
        }

        private void WelcomeWizard_Load(object sender, EventArgs e)
        {
            Log.Information("[WELCOMEWIZARD] 欢迎向导已启动");
        }

        /// <summary>
        /// 显示指定步骤
        /// </summary>
        private void ShowStep(WizardStep step)
        {
            _currentStep = step;

            // 清空主面板
            pnlMain.Controls.Clear();

            // 添加对应步骤的控件
            if (_stepControls.ContainsKey(step))
            {
                var control = _stepControls[step];
                control.Dock = DockStyle.Fill;
                pnlMain.Controls.Add(control);
            }

            // 更新标题
            UpdateTitle();

            Log.Debug("[WELCOMEWIZARD] 显示步骤: {Step}", step);
        }

        /// <summary>
        /// 更新窗口标题
        /// </summary>
        private void UpdateTitle()
        {
            switch (_currentStep)
            {
                case WizardStep.Welcome:
                    lblTitle.Text = "欢迎使用 HASS.Agent";
                    break;
                case WizardStep.QuickConfig:
                    lblTitle.Text = "快速配置";
                    break;
                case WizardStep.Discovery:
                    lblTitle.Text = "自动发现";
                    break;
                case WizardStep.AdvancedConfig:
                    lblTitle.Text = "高级配置";
                    break;
                case WizardStep.Progress:
                    lblTitle.Text = "正在配置...";
                    break;
                case WizardStep.Complete:
                    lblTitle.Text = "配置完成";
                    break;
            }
        }

        #region 步骤创建方法

        /// <summary>
        /// 创建欢迎步骤
        /// </summary>
        private UserControl CreateWelcomeStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var lblWelcome = new Label
            {
                Text = "🏠 欢迎使用 HASS.Agent!",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(50, 30)
            };

            var lblDescription = new Label
            {
                Text = "连接到您的 Home Assistant，开始监控和控制您的 Windows 电脑。\n\n请选择配置方式:",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(102, 102, 102),
                AutoSize = true,
                MaximumSize = new Size(700, 0),
                Location = new Point(50, 90)
            };

            // 快速配置按钮
            var btnQuickConfig = CreateOptionButton(
                "🚀 快速配置 (推荐)",
                "只需 2 个信息，2 分钟即可完成配置\n• Home Assistant 网址\n• 访问令牌",
                new Point(50, 180),
                () => ShowStep(WizardStep.QuickConfig)
            );

            // 高级配置按钮
            var btnAdvancedConfig = CreateOptionButton(
                "🔧 高级配置",
                "手动配置所有选项\n• MQTT 设置\n• WebSocket 配置\n• 传感器选择",
                new Point(400, 180),
                () => ShowStep(WizardStep.AdvancedConfig)
            );

            // 自动发现按钮
            var btnDiscovery = CreateOptionButton(
                "🔍 自动发现",
                "自动扫描网络中的 Home Assistant\n• 无需手动输入\n• 一键选择实例",
                new Point(50, 350),
                () => StartDiscovery()
            );

            panel.Controls.AddRange(new Control[]
            {
                lblWelcome, lblDescription, btnQuickConfig, btnAdvancedConfig, btnDiscovery
            });

            return panel;
        }

        /// <summary>
        /// 创建快速配置步骤
        /// </summary>
        private UserControl CreateQuickConfigStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var txtHaUrl = CreateTextBox("Home Assistant URL:", "http://homeassistant.local:8123", new Point(50, 50), 500);
            var txtToken = CreateTextBox("访问令牌:", "", new Point(50, 130), 500, true);
            var txtDeviceName = CreateTextBox("设备名称:", Environment.MachineName, new Point(50, 210), 500);

            var chkAutoSensors = new CheckBox
            {
                Text = "自动配置传感器 (推荐)",
                Checked = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(50, 270),
                AutoSize = true
            };

            var chkUseWebSocket = new CheckBox
            {
                Text = "使用 WebSocket (更稳定)",
                Checked = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(50, 300),
                AutoSize = true
            };

            // 按钮容器
            var btnContainer = new Panel
            {
                Location = new Point(0, 480),
                Size = new Size(800, 80),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var btnBack = new Button
            {
                Text = "← 返回",
                Size = new Size(100, 40),
                Location = new Point(50, 20),
                BackColor = Color.FromArgb(153, 153, 153),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
            };
            btnBack.Click += (s, e) => ShowStep(WizardStep.Welcome);

            var btnConnect = new Button
            {
                Text = "连接 →",
                Size = new Size(150, 40),
                Location = new Point(600, 20),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnConnect.Click += async (s, e) => await StartQuickConfig(txtHaUrl.Text, txtToken.Text);

            var btnGenerateToken = CreateLinkLabel("🔗 在 Home Assistant 中生成令牌", new Point(560, 145));

            btnContainer.Controls.AddRange(new Control[] { btnBack, btnConnect });

            panel.Controls.AddRange(new Control[]
            {
                txtHaUrl, txtToken, txtDeviceName, chkAutoSensors, chkUseWebSocket, btnGenerateToken, btnContainer
            });

            return panel;
        }

        /// <summary>
        /// 创建自动发现步骤
        /// </summary>
        private UserControl CreateDiscoveryStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var lblStatus = new Label
            {
                Text = "🔍 正在扫描网络中的 Home Assistant 实例...",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(50, 30),
                AutoSize = true
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(50, 80),
                Size = new Size(700, 30),
                Style = ProgressBarStyle.Continuous
            };

            var lblProgress = new Label
            {
                Text = "正在扫描 0/254...",
                Location = new Point(50, 120),
                AutoSize = true
            };

            var lstInstances = new ListBox
            {
                Location = new Point(50, 160),
                Size = new Size(700, 300),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnContainer = new Panel
            {
                Location = new Point(0, 480),
                Size = new Size(800, 80),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var btnBack = new Button
            {
                Text = "← 取消",
                Size = new Size(100, 40),
                Location = new Point(50, 20),
                BackColor = Color.FromArgb(153, 153, 153),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBack.Click += (s, e) => ShowStep(WizardStep.Welcome);

            var btnSelect = new Button
            {
                Text = "选择此实例 →",
                Size = new Size(150, 40),
                Location = new Point(600, 20),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            btnContainer.Controls.AddRange(new Control[] { btnBack, btnSelect });

            panel.Controls.AddRange(new Control[]
            {
                lblStatus, progressBar, lblProgress, lstInstances, btnContainer
            });

            return panel;
        }

        /// <summary>
        /// 创建高级配置步骤
        /// </summary>
        private UserControl CreateAdvancedConfigStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var lblMessage = new Label
            {
                Text = "🔧 高级配置\n\n\n高级配置模式正在开发中...\n\n请使用快速配置或等待更新。",
                Font = new Font("Segoe UI", 12F),
                Location = new Point(50, 50),
                AutoSize = true
            };

            var btnBack = new Button
            {
                Text = "← 返回",
                Size = new Size(100, 40),
                Location = new Point(50, 480),
                BackColor = Color.FromArgb(153, 153, 153),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBack.Click += (s, e) => ShowStep(WizardStep.Welcome);

            panel.Controls.AddRange(new Control[] { lblMessage, btnBack });

            return panel;
        }

        /// <summary>
        /// 创建进度步骤
        /// </summary>
        private UserControl CreateProgressStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var lblStatus = new Label
            {
                Text = "正在配置...",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(50, 30),
                AutoSize = true
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(50, 80),
                Size = new Size(700, 30)
            };

            var lblStep = new Label
            {
                Text = "准备中...",
                Location = new Point(50, 120),
                AutoSize = true
            };

            panel.Controls.AddRange(new Control[] { lblStatus, progressBar, lblStep });

            return panel;
        }

        /// <summary>
        /// 创建完成步骤
        /// </summary>
        private UserControl CreateCompleteStep()
        {
            var panel = new Panel { BackColor = Color.White };

            var lblSuccess = new Label
            {
                Text = "✅ 配置完成!",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(50, 50),
                AutoSize = true
            };

            var lblMessage = new Label
            {
                Text = "HASS.Agent 已成功配置并连接到 Home Assistant。\n\n您可以关闭此窗口，HASS.Agent 将在系统托盘中运行。",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(50, 120),
                MaximumSize = new Size(700, 0),
                AutoSize = true
            };

            var btnFinish = new Button
            {
                Text = "完成",
                Size = new Size(150, 50),
                Location = new Point(325, 250),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnFinish.Click += (s, e) => Close();

            panel.Controls.AddRange(new Control[] { lblSuccess, lblMessage, btnFinish });

            return panel;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建选项按钮
        /// </summary>
        private Button CreateOptionButton(string title, string description, Point location, Action onClick)
        {
            var btn = new Button
            {
                Size = new Size(320, 150),
                Location = location,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = $"{title}\n\n{description}"
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(204, 204, 204);
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(247, 249, 252);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.White;
            btn.Click += (s, e) => onClick?.Invoke();

            return btn;
        }

        /// <summary>
        /// 创建文本框
        /// </summary>
        private Control CreateTextBox(string labelText, string placeholder, Point location, int width, bool isPassword = false)
        {
            var panel = new Panel
            {
                Location = location,
                Size = new Size(width + 150, 60)
            };

            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true
            };

            var txt = new TextBox
            {
                Location = new Point(0, 25),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10F),
                Text = placeholder,
                UseSystemPasswordChar = isPassword
            };

            panel.Controls.AddRange(new Control[] { lbl, txt });
            return panel;
        }

        /// <summary>
        /// 创建链接标签
        /// </summary>
        private LinkLabel CreateLinkLabel(string text, Point location)
        {
            var link = new LinkLabel
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                LinkColor = Color.FromArgb(33, 150, 243)
            };
            link.LinkClicked += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://my.home-assistant.io/tokens",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[WELCOMEWIZARD] 打开链接失败");
                }
            };

            return link;
        }

        #endregion

        #region 配置方法

        /// <summary>
        /// 开始快速配置
        /// </summary>
        private async Task StartQuickConfig(string haUrl, string token)
        {
            if (string.IsNullOrWhiteSpace(haUrl) || string.IsNullOrWhiteSpace(token))
            {
                MessageBox.Show("请填写所有必填项", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowStep(WizardStep.Progress);

            try
            {
                using var configManager = new Managers.QuickConfigManager(haUrl, token);
                var result = await configManager.ExecuteQuickConfigureAsync();

                if (result.Success)
                {
                    ShowStep(WizardStep.Complete);
                }
                else
                {
                    MessageBox.Show($"配置失败: {result.ErrorMessage}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowStep(WizardStep.QuickConfig);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[WELCOMEWIZARD] 快速配置失败");
                MessageBox.Show($"配置失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStep(WizardStep.QuickConfig);
            }
        }

        /// <summary>
        /// 开始自动发现
        /// </summary>
        private async Task StartDiscovery()
        {
            ShowStep(WizardStep.Discovery);

            try
            {
                using var discoveryService = new Managers.HaDiscoveryService();
                var instances = await discoveryService.DiscoverInstancesAsync();

                if (instances.Any())
                {
                    Log.Information("[WELCOMEWIZARD] 发现 {Count} 个实例", instances.Count);
                    // TODO: 显示发现的实例列表
                }
                else
                {
                    MessageBox.Show("未发现 Home Assistant 实例。\n\n请使用快速配置手动输入。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowStep(WizardStep.Welcome);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[WELCOMEWIZARD] 自动发现失败");
                MessageBox.Show($"自动发现失败: {ex.Message}\n\n请使用快速配置。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStep(WizardStep.Welcome);
            }
        }

        #endregion
    }
}
