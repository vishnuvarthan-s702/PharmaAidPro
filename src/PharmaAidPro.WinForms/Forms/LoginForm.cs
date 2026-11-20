using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class LoginForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;
        private ComboBox _cboUserName;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private ComboBox _cboLocation;
        private ComboBox _cboFinYear;
        private Label _lblError;
        private Button _btnLogin;
        private Button _btnExit;

        public string LoggedInUser { get; private set; }
        public string SelectedLocation { get; private set; }
        public string SelectedFinYear { get; private set; }

        public LoginForm(PharmaRepository repo)
        {
            _repo = repo;
            _settings = _repo.GetStoreSettings();
            LoggedInUser = "Admin";
            SelectedLocation = "MAIN STORE";
            SelectedFinYear = GetCurrentFinYear();

            InitializeComponent();
            LoadUsers();
        }

        private static string GetCurrentFinYear()
        {
            var now = DateTime.Now;
            int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
            return string.Format("{0} - {1}", startYear, startYear + 1);
        }

        private void InitializeComponent()
        {
            this.Text = "The Nethaji Medical Stores - Login (Shopaid.Net-RE v7.5.16)";
            this.ClientSize = new Size(530, 365);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.BackColor = Color.FromArgb(235, 241, 248);
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            LoadingSplashForm.ApplyBrandingIcon(this);

            // Top Brand Header Banner
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.FromArgb(22, 66, 114)
            };

            var picLogo = new PictureBox
            {
                Bounds = new Rectangle(14, 6, 72, 72),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = LoadingSplashForm.LoadLogoBitmap()
            };

            var lblStoreTitle = new Label
            {
                Text = "The Nethaji Medical Stores",
                ForeColor = Color.White,
                Font = new Font("Tahoma", 15F, FontStyle.Bold),
                AutoSize = false,
                Bounds = new Rectangle(96, 10, 420, 28)
            };

            var lblStoreSub = new Label
            {
                Text = "CHEMIST & DRUGGIST  •  MEENAKSHIPURAM, NAGERCOIL",
                ForeColor = Color.FromArgb(255, 218, 120),
                Font = new Font("Tahoma", 8.5F, FontStyle.Bold),
                AutoSize = false,
                Bounds = new Rectangle(98, 39, 418, 18)
            };

            var lblLicBadge = new Label
            {
                Text = string.Format("License: {0}   |   Terminal: COUNTER-1   |   Edition: ESSENTIAL 7.5.16", _settings.LicenseRef),
                ForeColor = Color.FromArgb(200, 222, 245),
                Font = new Font("Tahoma", 8F, FontStyle.Regular),
                AutoSize = false,
                Bounds = new Rectangle(98, 58, 418, 16)
            };

            headerPanel.Controls.AddRange(new Control[] { picLogo, lblStoreTitle, lblStoreSub, lblLicBadge });

            // Center Login Card
            var cardPanel = new Panel
            {
                Bounds = new Rectangle(22, 98, 486, 186),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var labelFont = new Font("Tahoma", 9.5F, FontStyle.Bold);
            var inputFont = new Font("Tahoma", 10F, FontStyle.Bold);

            // 1. User Name
            var lblUser = new Label
            {
                Text = "User Name",
                Font = labelFont,
                ForeColor = Color.FromArgb(30, 50, 80),
                Bounds = new Rectangle(24, 18, 130, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _cboUserName = new ComboBox
            {
                Bounds = new Rectangle(162, 16, 276, 24),
                Font = inputFont,
                DropDownStyle = ComboBoxStyle.DropDown
            };

            // 2. Password
            var lblPass = new Label
            {
                Text = "Password",
                Font = labelFont,
                ForeColor = Color.FromArgb(30, 50, 80),
                Bounds = new Rectangle(24, 54, 130, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _txtPassword = new TextBox
            {
                Bounds = new Rectangle(162, 52, 206, 24),
                Font = inputFont,
                PasswordChar = '●',
                BackColor = Color.FromArgb(255, 255, 225)
            };

            _chkShowPassword = new CheckBox
            {
                Text = "Show",
                Bounds = new Rectangle(376, 54, 65, 22),
                Font = new Font("Tahoma", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 80, 105)
            };
            _chkShowPassword.CheckedChanged += delegate
            {
                _txtPassword.PasswordChar = _chkShowPassword.Checked ? '\0' : '●';
            };

            // 3. Location
            var lblLocation = new Label
            {
                Text = "Location",
                Font = labelFont,
                ForeColor = Color.FromArgb(30, 50, 80),
                Bounds = new Rectangle(24, 90, 130, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _cboLocation = new ComboBox
            {
                Bounds = new Rectangle(162, 88, 276, 24),
                Font = new Font("Tahoma", 9.5F, FontStyle.Regular),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboLocation.Items.AddRange(new object[] { "MAIN STORE", "GODOWN / RACK" });
            _cboLocation.SelectedIndex = 0;

            // 4. Financial Year
            var lblFinYear = new Label
            {
                Text = "Financial Year",
                Font = labelFont,
                ForeColor = Color.FromArgb(30, 50, 80),
                Bounds = new Rectangle(24, 126, 130, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _cboFinYear = new ComboBox
            {
                Bounds = new Rectangle(162, 124, 276, 24),
                Font = new Font("Tahoma", 9.5F, FontStyle.Regular),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            var currFy = GetCurrentFinYear();
            _cboFinYear.Items.AddRange(new object[] { currFy, "2025 - 2026", "2024 - 2025" });
            _cboFinYear.SelectedIndex = 0;

            _lblError = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(190, 20, 20),
                Font = new Font("Tahoma", 8.5F, FontStyle.Bold),
                Bounds = new Rectangle(24, 156, 416, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };

            cardPanel.Controls.AddRange(new Control[]
            {
                lblUser, _cboUserName,
                lblPass, _txtPassword, _chkShowPassword,
                lblLocation, _cboLocation,
                lblFinYear, _cboFinYear,
                _lblError
            });

            // Action Buttons
            _btnLogin = new Button
            {
                Text = "&Login (Enter)",
                Bounds = new Rectangle(252, 294, 124, 34),
                BackColor = Color.FromArgb(24, 108, 62),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9.5F, FontStyle.Bold)
            };
            _btnLogin.Click += delegate { PerformLogin(); };

            _btnExit = new Button
            {
                Text = "E&xit (Esc)",
                Bounds = new Rectangle(384, 294, 104, 34),
                BackColor = Color.FromArgb(145, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9F, FontStyle.Bold)
            };
            _btnExit.Click += delegate
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            var lblHint = new Label
            {
                Text = "Default Password: SHOPAID",
                ForeColor = Color.FromArgb(80, 100, 125),
                Font = new Font("Tahoma", 8.25F, FontStyle.Italic),
                Bounds = new Rectangle(22, 302, 220, 20)
            };

            // Bottom Status Bar
            var statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(224, 232, 242),
                SizingGrip = false
            };
            statusStrip.Items.Add(new ToolStripStatusLabel("DB: Connected (Local SQLite WAL)") { Spring = true, TextAlign = ContentAlignment.MiddleLeft });
            statusStrip.Items.Add(new ToolStripStatusLabel(DateTime.Now.ToString("dd/MM/yyyy")));

            this.Controls.AddRange(new Control[]
            {
                headerPanel, cardPanel, lblHint, _btnLogin, _btnExit, statusStrip
            });

            // Keyboard handling
            _cboUserName.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _txtPassword.Focus();
                    _txtPassword.SelectAll();
                }
            };

            _txtPassword.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    PerformLogin();
                }
            };

            _cboLocation.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _cboFinYear.Focus();
                }
            };

            _cboFinYear.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    PerformLogin();
                }
            };

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            this.Shown += delegate
            {
                _txtPassword.Focus();
                _txtPassword.SelectAll();
            };
        }

        private void LoadUsers()
        {
            List<AppUser> users = _repo.GetActiveUsers();
            _cboUserName.Items.Clear();
            foreach (var u in users)
            {
                _cboUserName.Items.Add(u.UserName);
            }
            if (_cboUserName.Items.Count > 0)
            {
                _cboUserName.SelectedIndex = 0;
            }
            else
            {
                _cboUserName.Text = "Admin";
            }
        }

        private void PerformLogin()
        {
            string user = (_cboUserName.Text ?? string.Empty).Trim();
            string pass = (_txtPassword.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(user))
            {
                _lblError.Text = "Please enter User Name.";
                _cboUserName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(pass))
            {
                _lblError.Text = "Please enter Password (Default: SHOPAID).";
                _txtPassword.Focus();
                return;
            }

            var matched = _repo.ValidateUser(user, pass);
            if (matched == null)
            {
                _lblError.Text = "Invalid User Name or Password! (Default: Admin / SHOPAID)";
                _txtPassword.Focus();
                _txtPassword.SelectAll();
                return;
            }

            LoggedInUser = matched.UserName;
            SelectedLocation = _cboLocation.Text;
            SelectedFinYear = _cboFinYear.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
