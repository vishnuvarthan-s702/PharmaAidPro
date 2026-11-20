using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class SettingsAndBackupForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;

        private TextBox _txtStoreName;
        private TextBox _txtAddress;
        private TextBox _txtDlNo;
        private TextBox _txtGstin;
        private ComboBox _cmbPrinter;
        private TextBox _txtBackupDir;

        public event Action SettingsUpdated;

        public SettingsAndBackupForm(PharmaRepository repo)
        {
            _repo = repo;
            _settings = _repo.GetStoreSettings();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Store Settings, Printer, Backup & Legacy Import";
            this.Size = new Size(620, 465);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 244, 249);

            int y = 18;
            _txtStoreName = AddRow("Store Name:", _settings.StoreName, ref y);
            _txtAddress = AddRow("Address:", _settings.Address, ref y);
            _txtDlNo = AddRow("Drug License No:", _settings.DrugLicenseNo, ref y);
            _txtGstin = AddRow("GSTIN:", _settings.Gstin, ref y);

            this.Controls.Add(new Label { Text = "Dot-Matrix Printer:", Left = 18, Top = y + 3, Width = 135 });
            _cmbPrinter = new ComboBox { Left = 160, Top = y, Width = 420, DropDownStyle = ComboBoxStyle.DropDown };
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                _cmbPrinter.Items.Add(printer);
            }
            _cmbPrinter.Text = _settings.PrinterName ?? string.Empty;
            this.Controls.Add(_cmbPrinter);
            y += 34;

            _txtBackupDir = AddRow("Backup Folder:", _settings.BackupDirectory ?? string.Empty, ref y);
            _txtBackupDir.Width = 330;
            var btnBrowse = new Button { Text = "Browse...", Left = 500, Top = y - 35, Width = 80, Height = 25 };
            btnBrowse.Click += delegate
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK) _txtBackupDir.Text = fbd.SelectedPath;
                }
            };
            this.Controls.Add(btnBrowse);

            var btnSave = new Button { Text = "Save Settings", Left = 160, Top = y + 10, Width = 130, Height = 32, BackColor = Color.FromArgb(215, 232, 250) };
            btnSave.Click += BtnSave_Click;

            var btnBackupNow = new Button { Text = "Backup Database Now", Left = 300, Top = y + 10, Width = 155, Height = 32 };
            btnBackupNow.Click += BtnBackupNow_Click;

            var btnImportCsv = new Button { Text = "Import Products CSV", Left = 160, Top = y + 52, Width = 295, Height = 30 };
            btnImportCsv.Click += BtnImportCsv_Click;

            var btnSyncOldApp = new Button
            {
                Text = "1-Click Sync Live Stock && Medicines from Old Shopaid App",
                Left = 160,
                Top = y + 92,
                Width = 420,
                Height = 36,
                BackColor = Color.FromArgb(24, 115, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9F, FontStyle.Bold)
            };
            btnSyncOldApp.Click += BtnSyncOldApp_Click;

            var btnClose = new Button { Text = "Close", Left = 490, Top = y + 10, Width = 90, Height = 32 };
            btnClose.Click += delegate { this.Close(); };

            this.Controls.AddRange(new Control[] { btnSave, btnBackupNow, btnImportCsv, btnSyncOldApp, btnClose });

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) this.Close();
            };
        }

        private TextBox AddRow(string label, string value, ref int y)
        {
            this.Controls.Add(new Label { Text = label, Left = 18, Top = y + 3, Width = 135 });
            var tb = new TextBox { Left = 160, Top = y, Width = 420, Text = value ?? string.Empty };
            this.Controls.Add(tb);
            y += 34;
            return tb;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _settings.StoreName = _txtStoreName.Text.Trim();
            _settings.Address = _txtAddress.Text.Trim();
            _settings.DrugLicenseNo = _txtDlNo.Text.Trim();
            _settings.Gstin = _txtGstin.Text.Trim();
            _settings.PrinterName = _cmbPrinter.Text.Trim();
            _settings.BackupDirectory = _txtBackupDir.Text.Trim();

            _repo.UpdateStoreSettings(_settings);
            if (SettingsUpdated != null) SettingsUpdated();
            MessageBox.Show("Settings saved.", "PharmaAid Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnBackupNow_Click(object sender, EventArgs e)
        {
            string path = _repo.BackupDatabase(_txtBackupDir.Text.Trim());
            MessageBox.Show("Database backed up safely to:\n" + path, "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnImportCsv_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "Select Legacy Products CSV (Code,Name,PurUnit,PurPrice,SaleUnit,MRP,HSN,Tax%)",
                Filter = "CSV Files (*.csv)|*.csv"
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                int imported = _repo.ImportProductsFromCsv(ofd.FileName);
                if (SettingsUpdated != null) SettingsUpdated();
                MessageBox.Show(string.Format("Imported {0} products into SQLite.", imported), "Legacy Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSyncOldApp_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int p, b, s;
            string msg;
            bool ok = _repo.SyncFromLegacyShopaidSqlServer(null, out p, out b, out s, out msg);
            Cursor.Current = Cursors.Default;
            if (SettingsUpdated != null) SettingsUpdated();
            MessageBox.Show(msg, ok ? "Old Shopaid Sync Complete" : "Shopaid Sync Info", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
