using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("The Nethaji Medical Stores - PharmaAid Pro Setup")]
[assembly: AssemblyProduct("PharmaAid Pro Installer")]
[assembly: AssemblyVersion("7.5.16.0")]

namespace PharmaAidPro.Setup
{
    internal static class InstallerProgram
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args != null && args.Length > 0 && args[0] == "--verify-installer")
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
                {
                    if (stream == null)
                    {
                        Console.WriteLine("INSTALLER_ERR|NO_PAYLOAD");
                        return 1;
                    }
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        Console.WriteLine("INSTALLER_OK|Entries=" + archive.Entries.Count + "|Bytes=" + stream.Length);
                        return 0;
                    }
                }
            }

            Application.Run(new SetupWizardForm());
            return 0;
        }
    }

    public class SetupWizardForm : Form
    {
        private TextBox _txtInstallPath;
        private CheckBox _chkDesktopShortcut;
        private CheckBox _chkStartMenuShortcut;
        private CheckBox _chkLaunchNow;
        private ProgressBar _progressBar;
        private Label _lblStatus;
        private Button _btnInstall;
        private Button _btnCancel;
        private Button _btnBrowse;

        public SetupWizardForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "The Nethaji Medical Stores — PharmaAid Pro Setup (v7.5.16)";
            this.ClientSize = new Size(560, 410);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(236, 241, 248);
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
            }

            // Header Banner
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.FromArgb(20, 62, 108)
            };

            var lblTitle = new Label
            {
                Text = "The Nethaji Medical Stores",
                ForeColor = Color.White,
                Font = new Font("Tahoma", 15.5F, FontStyle.Bold),
                Bounds = new Rectangle(20, 12, 520, 28)
            };

            var lblSubtitle = new Label
            {
                Text = "CHEMIST & DRUGGIST  •  MEENAKSHIPURAM, NAGERCOIL",
                ForeColor = Color.FromArgb(255, 216, 110),
                Font = new Font("Tahoma", 8.75F, FontStyle.Bold),
                Bounds = new Rectangle(22, 42, 520, 18)
            };

            var lblVersion = new Label
            {
                Text = "PharmaAid Pro Offline ERP Setup Wizard  |  32-bit (x86) Windows 7 / 10 / 11",
                ForeColor = Color.FromArgb(205, 226, 248),
                Font = new Font("Tahoma", 8.25F, FontStyle.Regular),
                Bounds = new Rectangle(22, 62, 520, 16)
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblVersion });

            // Body Card
            var card = new Panel
            {
                Bounds = new Rectangle(20, 104, 520, 232),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFolderTitle = new Label
            {
                Text = "Installation Folder:",
                Font = new Font("Tahoma", 9.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 48, 76),
                Bounds = new Rectangle(18, 16, 480, 20)
            };

            _txtInstallPath = new TextBox
            {
                Text = @"C:\PharmaAidPro",
                Font = new Font("Tahoma", 9.5F, FontStyle.Regular),
                Bounds = new Rectangle(18, 40, 392, 24)
            };

            _btnBrowse = new Button
            {
                Text = "Browse...",
                Bounds = new Rectangle(418, 39, 82, 26),
                BackColor = Color.FromArgb(230, 236, 245)
            };
            _btnBrowse.Click += BtnBrowse_Click;

            _chkDesktopShortcut = new CheckBox
            {
                Text = "Create Desktop Shortcut (\"The Nethaji Medical Stores - PharmaAid Pro\")",
                Checked = true,
                Font = new Font("Tahoma", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 50, 80),
                Bounds = new Rectangle(18, 80, 480, 22)
            };

            _chkStartMenuShortcut = new CheckBox
            {
                Text = "Create Start Menu Program Shortcut",
                Checked = true,
                Font = new Font("Tahoma", 9F, FontStyle.Regular),
                Bounds = new Rectangle(18, 108, 480, 22)
            };

            _chkLaunchNow = new CheckBox
            {
                Text = "Launch PharmaAid Pro immediately after installation completes",
                Checked = true,
                Font = new Font("Tahoma", 9F, FontStyle.Regular),
                Bounds = new Rectangle(18, 136, 480, 22)
            };

            _lblStatus = new Label
            {
                Text = "Ready to install. Existing database (Data\\PharmaAidPro.db) is always preserved.",
                ForeColor = Color.FromArgb(60, 85, 115),
                Font = new Font("Tahoma", 8.5F, FontStyle.Regular),
                Bounds = new Rectangle(18, 170, 482, 20)
            };

            _progressBar = new ProgressBar
            {
                Bounds = new Rectangle(18, 194, 482, 22),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            card.Controls.AddRange(new Control[]
            {
                lblFolderTitle, _txtInstallPath, _btnBrowse,
                _chkDesktopShortcut, _chkStartMenuShortcut, _chkLaunchNow,
                _lblStatus, _progressBar
            });

            _btnInstall = new Button
            {
                Text = "&Install Now",
                Bounds = new Rectangle(296, 352, 130, 36),
                BackColor = Color.FromArgb(24, 115, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9.75F, FontStyle.Bold)
            };
            _btnInstall.Click += BtnInstall_Click;

            _btnCancel = new Button
            {
                Text = "Close",
                Bounds = new Rectangle(436, 352, 104, 36),
                BackColor = Color.FromArgb(145, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9F, FontStyle.Bold)
            };
            _btnCancel.Click += delegate { this.Close(); };

            var lblFooterNote = new Label
            {
                Text = "Default Login: Admin / SHOPAID",
                ForeColor = Color.FromArgb(80, 100, 125),
                Font = new Font("Tahoma", 8.5F, FontStyle.Italic),
                Bounds = new Rectangle(22, 362, 250, 20)
            };

            this.Controls.AddRange(new Control[]
            {
                headerPanel, card, lblFooterNote, _btnInstall, _btnCancel
            });
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select folder to install PharmaAid Pro:";
                fbd.SelectedPath = _txtInstallPath.Text;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    _txtInstallPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            string targetDir = (_txtInstallPath.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Please choose a valid installation folder.", "Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _btnInstall.Enabled = false;
                _btnBrowse.Enabled = false;
                _txtInstallPath.ReadOnly = true;

                // Stop any running PharmaAidPro instance before overwriting binaries
                foreach (var proc in Process.GetProcessesByName("PharmaAidPro"))
                {
                    try { proc.Kill(); proc.WaitForExit(2000); } catch { }
                }

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException("Embedded application payload was not found.");
                    }

                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        int total = archive.Entries.Count;
                        int current = 0;

                        foreach (var entry in archive.Entries)
                        {
                            current++;
                            _progressBar.Value = Math.Min(100, (int)Math.Round((current * 90.0) / Math.Max(1, total)));

                            string relPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                            string destPath = Path.Combine(targetDir, relPath);

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destPath);
                                continue;
                            }

                            // Never overwrite an existing store database
                            if (relPath.EndsWith("PharmaAidPro.db", StringComparison.OrdinalIgnoreCase) && File.Exists(destPath))
                            {
                                continue;
                            }

                            string parentDir = Path.GetDirectoryName(destPath);
                            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                            {
                                Directory.CreateDirectory(parentDir);
                            }

                            _lblStatus.Text = "Installing: " + relPath;
                            Application.DoEvents();

                            using (var inStream = entry.Open())
                            using (var outStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                inStream.CopyTo(outStream);
                            }
                        }
                    }
                }

                string exePath = Path.Combine(targetDir, "PharmaAidPro.exe");

                if (_chkDesktopShortcut.Checked)
                {
                    string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    CreateWindowsShortcut(
                        Path.Combine(desktopDir, "The Nethaji Medical Stores - PharmaAid Pro.lnk"),
                        exePath,
                        targetDir,
                        "The Nethaji Medical Stores - PharmaAid Pro ERP");
                }

                if (_chkStartMenuShortcut.Checked)
                {
                    string programsDir = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    string folder = Path.Combine(programsDir, "The Nethaji Medical Stores");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    CreateWindowsShortcut(
                        Path.Combine(folder, "PharmaAid Pro.lnk"),
                        exePath,
                        targetDir,
                        "The Nethaji Medical Stores - PharmaAid Pro ERP");
                }

                _progressBar.Value = 100;
                _lblStatus.Text = "Installation completed successfully! Installed to: " + targetDir;
                _lblStatus.ForeColor = Color.FromArgb(20, 115, 55);

                MessageBox.Show(
                    "PharmaAid Pro has been installed successfully!\n\n" +
                    "Folder: " + targetDir + "\n" +
                    "Default Login: Admin\n" +
                    "Default Password: SHOPAID",
                    "Installation Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (_chkLaunchNow.Checked && File.Exists(exePath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = targetDir,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                _btnInstall.Enabled = true;
                _btnBrowse.Enabled = true;
                _txtInstallPath.ReadOnly = false;
                _lblStatus.Text = "Error: " + ex.Message;
                _lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Installation failed:\n" + ex.Message, "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateWindowsShortcut(string shortcutPath, string targetExe, string workingDir, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                if (shortcut == null) return;

                Type scType = shortcut.GetType();
                scType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetExe });
                scType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDir });
                scType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                scType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { targetExe + ",0" });
                scType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            catch
            {
            }
        }
    }
}
