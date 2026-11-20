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
                using (Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.pak"))
                {
                    if (res == null)
                    {
                        Console.WriteLine("INSTALLER_ERR|NO_PAYLOAD");
                        return 1;
                    }
                    using (GZipStream gz = new GZipStream(res, CompressionMode.Decompress))
                    using (BinaryReader br = new BinaryReader(gz))
                    {
                        int count = br.ReadInt32();
                        Console.WriteLine("INSTALLER_OK|Entries=" + count + "|CompressedBytes=" + res.Length);
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
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 88;
            headerPanel.BackColor = Color.FromArgb(20, 62, 108);

            PictureBox picLogo = new PictureBox();
            picLogo.Bounds = new Rectangle(14, 6, 76, 76);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.BackColor = Color.Transparent;
            try
            {
                using (Stream logoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app_logo.jpg"))
                {
                    if (logoStream != null)
                    {
                        using (Image tempImg = Image.FromStream(logoStream))
                        {
                            Bitmap bmp = new Bitmap(tempImg.Width, tempImg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.DrawImage(tempImg, 0, 0, tempImg.Width, tempImg.Height);
                            }
                            picLogo.Image = bmp;
                        }
                    }
                }
            }
            catch
            {
            }

            Label lblTitle = new Label();
            lblTitle.Text = "The Nethaji Medical Stores";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Tahoma", 15.5F, FontStyle.Bold);
            lblTitle.Bounds = new Rectangle(100, 12, 445, 28);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "CHEMIST & DRUGGIST  •  MEENAKSHIPURAM, NAGERCOIL";
            lblSubtitle.ForeColor = Color.FromArgb(255, 216, 110);
            lblSubtitle.Font = new Font("Tahoma", 8.75F, FontStyle.Bold);
            lblSubtitle.Bounds = new Rectangle(102, 42, 443, 18);

            Label lblVersion = new Label();
            lblVersion.Text = "PharmaAid Pro Offline ERP Setup Wizard  |  Windows 7 (32-bit x86) & Windows 10/11";
            lblVersion.ForeColor = Color.FromArgb(205, 226, 248);
            lblVersion.Font = new Font("Tahoma", 8.25F, FontStyle.Regular);
            lblVersion.Bounds = new Rectangle(102, 62, 443, 16);

            headerPanel.Controls.AddRange(new Control[] { picLogo, lblTitle, lblSubtitle, lblVersion });

            // Body Card
            Panel card = new Panel();
            card.Bounds = new Rectangle(20, 104, 520, 232);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblFolderTitle = new Label();
            lblFolderTitle.Text = "Installation Folder:";
            lblFolderTitle.Font = new Font("Tahoma", 9.25F, FontStyle.Bold);
            lblFolderTitle.ForeColor = Color.FromArgb(28, 48, 76);
            lblFolderTitle.Bounds = new Rectangle(18, 16, 480, 20);

            _txtInstallPath = new TextBox();
            _txtInstallPath.Text = Directory.Exists(@"D:\") ? @"D:\PharmaAidPro" : @"C:\PharmaAidPro";
            _txtInstallPath.Font = new Font("Tahoma", 9.5F, FontStyle.Regular);
            _txtInstallPath.Bounds = new Rectangle(18, 40, 392, 24);

            _btnBrowse = new Button();
            _btnBrowse.Text = "Browse...";
            _btnBrowse.Bounds = new Rectangle(418, 39, 82, 26);
            _btnBrowse.BackColor = Color.FromArgb(230, 236, 245);
            _btnBrowse.Click += BtnBrowse_Click;

            _chkDesktopShortcut = new CheckBox();
            _chkDesktopShortcut.Text = "Create Desktop Shortcut (\"The Nethaji Medical Stores - PharmaAid Pro\")";
            _chkDesktopShortcut.Checked = true;
            _chkDesktopShortcut.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _chkDesktopShortcut.ForeColor = Color.FromArgb(25, 50, 80);
            _chkDesktopShortcut.Bounds = new Rectangle(18, 80, 480, 22);

            _chkStartMenuShortcut = new CheckBox();
            _chkStartMenuShortcut.Text = "Create Start Menu Program Shortcut";
            _chkStartMenuShortcut.Checked = true;
            _chkStartMenuShortcut.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            _chkStartMenuShortcut.Bounds = new Rectangle(18, 108, 480, 22);

            _chkLaunchNow = new CheckBox();
            _chkLaunchNow.Text = "Launch PharmaAid Pro immediately after installation completes";
            _chkLaunchNow.Checked = true;
            _chkLaunchNow.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            _chkLaunchNow.Bounds = new Rectangle(18, 136, 480, 22);

            _lblStatus = new Label();
            _lblStatus.Text = "Ready to install. Existing database (Data\\PharmaAidPro.db) is always preserved.";
            _lblStatus.ForeColor = Color.FromArgb(60, 85, 115);
            _lblStatus.Font = new Font("Tahoma", 8.5F, FontStyle.Regular);
            _lblStatus.Bounds = new Rectangle(18, 170, 482, 20);

            _progressBar = new ProgressBar();
            _progressBar.Bounds = new Rectangle(18, 194, 482, 22);
            _progressBar.Minimum = 0;
            _progressBar.Maximum = 100;
            _progressBar.Value = 0;

            card.Controls.AddRange(new Control[]
            {
                lblFolderTitle, _txtInstallPath, _btnBrowse,
                _chkDesktopShortcut, _chkStartMenuShortcut, _chkLaunchNow,
                _lblStatus, _progressBar
            });

            _btnInstall = new Button();
            _btnInstall.Text = "&Install Now";
            _btnInstall.Bounds = new Rectangle(296, 352, 130, 36);
            _btnInstall.BackColor = Color.FromArgb(24, 115, 65);
            _btnInstall.ForeColor = Color.White;
            _btnInstall.FlatStyle = FlatStyle.Flat;
            _btnInstall.Font = new Font("Tahoma", 9.75F, FontStyle.Bold);
            _btnInstall.Click += BtnInstall_Click;

            _btnCancel = new Button();
            _btnCancel.Text = "Close";
            _btnCancel.Bounds = new Rectangle(436, 352, 104, 36);
            _btnCancel.BackColor = Color.FromArgb(145, 45, 45);
            _btnCancel.ForeColor = Color.White;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _btnCancel.Click += delegate { this.Close(); };

            Label lblFooterNote = new Label();
            lblFooterNote.Text = "Default Login: Admin / pass";
            lblFooterNote.ForeColor = Color.FromArgb(80, 100, 125);
            lblFooterNote.Font = new Font("Tahoma", 8.5F, FontStyle.Italic);
            lblFooterNote.Bounds = new Rectangle(22, 362, 250, 20);

            this.Controls.AddRange(new Control[]
            {
                headerPanel, card, lblFooterNote, _btnInstall, _btnCancel
            });
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
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
            if (targetDir.Length == 0)
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
                foreach (Process proc in Process.GetProcessesByName("PharmaAidPro"))
                {
                    try { proc.Kill(); proc.WaitForExit(2000); } catch { }
                }

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                using (Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.pak"))
                {
                    if (res == null)
                    {
                        throw new InvalidOperationException("Embedded application payload was not found.");
                    }

                    using (GZipStream gz = new GZipStream(res, CompressionMode.Decompress))
                    using (BinaryReader br = new BinaryReader(gz))
                    {
                        int total = br.ReadInt32();
                        for (int i = 0; i < total; i++)
                        {
                            string relPath = br.ReadString().Replace('/', Path.DirectorySeparatorChar);
                            int byteLen = br.ReadInt32();
                            byte[] data = br.ReadBytes(byteLen);

                            _progressBar.Value = Math.Min(100, (int)Math.Round(((i + 1) * 90.0) / Math.Max(1, total)));

                            string destPath = Path.Combine(targetDir, relPath);

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

                            File.WriteAllBytes(destPath, data);
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
                    "Default Password: pass",
                    "Installation Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (_chkLaunchNow.Checked && File.Exists(exePath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = exePath;
                    psi.WorkingDirectory = targetDir;
                    psi.UseShellExecute = true;
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
