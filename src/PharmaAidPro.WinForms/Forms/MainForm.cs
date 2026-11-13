using System;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;
        private MenuStrip _menuStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusHintLabel;
        private ToolStripStatusLabel _statusStoreLabel;
        private ToolStripStatusLabel _statusDbLabel;
        private ToolStripStatusLabel _statusClockLabel;
        private Timer _clockTimer;

        public MainForm()
        {
            _repo = new PharmaRepository();
            _settings = _repo.GetStoreSettings();

            InitializeComponent();
            UpdateStatusBar();
        }

        private void InitializeComponent()
        {
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 680);
            this.BackColor = Color.FromArgb(228, 234, 242);
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            this.Text = string.Format("{0} - PharmaAid Pro (Offline Edition)", _settings.StoreName);

            BuildMenuStrip();
            BuildStatusStrip();

            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += delegate
            {
                _statusClockLabel.Text = DateTime.Now.ToString("dd/MM/yy  hh:mm:ss tt");
            };
            _clockTimer.Start();

            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                OpenNewProductModal();
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                e.Handled = true;
                OpenPurchaseEntry();
            }
        }

        private void OpenPurchaseEntry()
        {
            var frm = new PurchaseEntryForm(_repo);
            frm.GrnSaved += UpdateStatusBar;
            frm.ShowDialog(this);
        }

        private void OpenNewProductModal()
        {
            using (var dlg = new NewProductForm(_repo))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateStatusBar();
                    MessageBox.Show(
                        string.Format("Product Saved: {0} ({1})", dlg.SavedProduct.ProductName, dlg.SavedProduct.ProductCode),
                        "NewProduct",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void OpenProductMaster()
        {
            var frm = new ProductMasterForm(_repo);
            frm.DataChanged += UpdateStatusBar;
            frm.ShowDialog(this);
        }

        private void OpenSupplierMaster()
        {
            var frm = new SupplierMasterForm(_repo);
            frm.DataChanged += UpdateStatusBar;
            frm.ShowDialog(this);
        }

        private void BuildMenuStrip()
        {
            _menuStrip = new MenuStrip();
            _menuStrip.BackColor = Color.FromArgb(240, 244, 249);
            _menuStrip.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("Store Settings", null, delegate { ShowComingSoon("Store Settings"); });
            fileMenu.DropDownItems.Add("Backup Database", null, delegate { ShowComingSoon("Backup Database"); });
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, delegate { this.Close(); });

            var salesMenu = new ToolStripMenuItem("&Sales");
            salesMenu.DropDownItems.Add("Retail Sales Bill (F2)", null, delegate { ShowComingSoon("Retail Sales Bill (F2)"); });
            salesMenu.DropDownItems.Add("Bill Copy / Reprint (F8)", null, delegate { ShowComingSoon("Bill Copy / Reprint"); });

            var purchaseMenu = new ToolStripMenuItem("&Purchase");
            purchaseMenu.DropDownItems.Add("Purchase Invoice (Ctrl+I)", null, delegate { OpenPurchaseEntry(); });
            purchaseMenu.DropDownItems.Add("New Product Master (Ctrl+N)", null, delegate { OpenNewProductModal(); });

            var orderMenu = new ToolStripMenuItem("&Order Management");
            var accountsMenu = new ToolStripMenuItem("&Accounts");
            var inventoryMenu = new ToolStripMenuItem("&Inventory");
            inventoryMenu.DropDownItems.Add("Product & Stock Master", null, delegate { OpenProductMaster(); });
            inventoryMenu.DropDownItems.Add("Supplier Master", null, delegate { OpenSupplierMaster(); });

            var reportsMenu = new ToolStripMenuItem("&Reports");
            var maintenanceMenu = new ToolStripMenuItem("&Maintenance");
            var windowsMenu = new ToolStripMenuItem("&Windows");
            var gstMenu = new ToolStripMenuItem("&GST");

            _menuStrip.Items.AddRange(new ToolStripItem[]
            {
                fileMenu, salesMenu, purchaseMenu, orderMenu,
                accountsMenu, inventoryMenu, reportsMenu,
                maintenanceMenu, windowsMenu, gstMenu
            });

            this.MainMenuStrip = _menuStrip;
            this.Controls.Add(_menuStrip);
        }

        private void BuildStatusStrip()
        {
            _statusStrip = new StatusStrip();
            _statusStrip.BackColor = Color.FromArgb(232, 237, 245);

            _statusHintLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Ready | F2: Sales Bill  |  Ctrl+I: Purchase Invoice  |  Ctrl+N: New Product  |  Esc: Close"
            };

            _statusDbLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Padding = new Padding(8, 0, 8, 0)
            };

            _statusStoreLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Padding = new Padding(8, 0, 8, 0),
                Text = string.Format("{0} | {1}:{2}", _settings.LicenseRef, _settings.DefaultUser, 1)
            };

            _statusClockLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Padding = new Padding(8, 0, 8, 0),
                Text = DateTime.Now.ToString("dd/MM/yy  hh:mm:ss tt")
            };

            _statusStrip.Items.AddRange(new ToolStripItem[]
            {
                _statusHintLabel, _statusDbLabel, _statusStoreLabel, _statusClockLabel
            });

            this.Controls.Add(_statusStrip);
        }

        private void UpdateStatusBar()
        {
            int products, batches, suppliers;
            _repo.GetDatabaseCounts(out products, out batches, out suppliers);
            _statusDbLabel.Text = string.Format("SQLite WAL | Products: {0} | Batches: {1} | Suppliers: {2}", products, batches, suppliers);
        }

        private void ShowComingSoon(string feature)
        {
            MessageBox.Show(
                string.Format("{0} will be implemented in the upcoming phase.", feature),
                "PharmaAid Pro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
