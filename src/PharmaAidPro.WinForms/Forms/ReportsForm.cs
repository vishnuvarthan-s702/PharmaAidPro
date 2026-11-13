using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class ReportsForm : Form
    {
        private readonly PharmaRepository _repo;
        private TabControl _tabs;
        private DataGridView _salesGrid;
        private DataGridView _purchaseGrid;
        private DataGridView _stockGrid;
        private ComboBox _cmbExpiryFilter;
        private Label _lblGstSummary;

        public ReportsForm(PharmaRepository repo, int initialTab = 0)
        {
            _repo = repo;
            InitializeComponent();
            LoadAllReports();
            if (initialTab >= 0 && initialTab < _tabs.TabPages.Count)
            {
                _tabs.SelectedIndex = initialTab;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Reports & GST Summary";
            this.Size = new Size(1020, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            _tabs = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Sales Register
            var tabSales = new TabPage("Sales Register");
            _salesGrid = CreateReadOnlyGrid();
            var btnExpSales = new Button { Text = "Export Sales CSV", Dock = DockStyle.Top, Height = 30 };
            btnExpSales.Click += delegate { ExportGridToCsv(_salesGrid, "SalesRegister.csv"); };
            tabSales.Controls.Add(_salesGrid);
            tabSales.Controls.Add(btnExpSales);

            // Tab 2: Purchase Register
            var tabPurchase = new TabPage("Purchase / GRN Register");
            _purchaseGrid = CreateReadOnlyGrid();
            var btnExpPurch = new Button { Text = "Export Purchase CSV", Dock = DockStyle.Top, Height = 30 };
            btnExpPurch.Click += delegate { ExportGridToCsv(_purchaseGrid, "PurchaseRegister.csv"); };
            tabPurchase.Controls.Add(_purchaseGrid);
            tabPurchase.Controls.Add(btnExpPurch);

            // Tab 3: Stock & Near-Expiry
            var tabStock = new TabPage("Stock & Near-Expiry");
            var stockTop = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(6) };
            stockTop.Controls.Add(new Label { Text = "Expiry Window:", Left = 8, Top = 10, Width = 96 });
            _cmbExpiryFilter = new ComboBox { Left = 108, Top = 7, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbExpiryFilter.Items.AddRange(new object[] { "All Active Stock", "Expiring within 3 Months", "Expiring within 6 Months", "Expiring within 12 Months" });
            _cmbExpiryFilter.SelectedIndex = 0;
            _cmbExpiryFilter.SelectedIndexChanged += delegate { LoadStockReport(); };
            var btnExpStock = new Button { Text = "Export Stock CSV", Left = 290, Top = 5, Width = 140, Height = 26 };
            btnExpStock.Click += delegate { ExportGridToCsv(_stockGrid, "StockExpiryReport.csv"); };
            stockTop.Controls.AddRange(new Control[] { _cmbExpiryFilter, btnExpStock });

            _stockGrid = CreateReadOnlyGrid();
            tabStock.Controls.Add(_stockGrid);
            tabStock.Controls.Add(stockTop);

            // Tab 4: GST Summary
            var tabGst = new TabPage("GST Summary (Sales vs Purchase)");
            _lblGstSummary = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Font = new Font("Consolas", 11F, FontStyle.Regular),
                BackColor = Color.White
            };
            tabGst.Controls.Add(_lblGstSummary);

            _tabs.TabPages.AddRange(new TabPage[] { tabSales, tabPurchase, tabStock, tabGst });
            this.Controls.Add(_tabs);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) this.Close();
            };
        }

        private static DataGridView CreateReadOnlyGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
        }

        private void LoadAllReports()
        {
            List<SalesHeader> sales = _repo.GetRecentSales(string.Empty);
            _salesGrid.DataSource = sales;

            List<PurchaseHeader> purchases = _repo.GetRecentPurchases(string.Empty);
            _purchaseGrid.DataSource = purchases;

            LoadStockReport();

            decimal salesBase = 0m, salesSgst = 0m, salesCgst = 0m, salesNet = 0m;
            foreach (var s in sales)
            {
                salesBase += s.BaseAmt;
                salesSgst += s.SgstAmt;
                salesCgst += s.CgstAmt;
                salesNet += s.NetAmount;
            }

            decimal purGross = 0m, purSgst = 0m, purCgst = 0m, purNet = 0m;
            foreach (var p in purchases)
            {
                purGross += p.GrossAmt;
                purSgst += p.SgstAmt;
                purCgst += p.CgstAmt;
                purNet += p.NetAmt;
            }

            var sb = new StringBuilder();
            sb.AppendLine("==========================================================");
            sb.AppendLine("            The Nethaji Medical Stores - GST SUMMARY");
            sb.AppendLine("==========================================================");
            sb.AppendLine();
            sb.AppendLine(string.Format("OUTPUT TAX (RETAIL SALES - {0} Bills)", sales.Count));
            sb.AppendLine(string.Format("  Taxable Base Value : Rs {0,12:0.00}", salesBase));
            sb.AppendLine(string.Format("  Output SGST        : Rs {0,12:0.00}", salesSgst));
            sb.AppendLine(string.Format("  Output CGST        : Rs {0,12:0.00}", salesCgst));
            sb.AppendLine(string.Format("  Total Sales Value  : Rs {0,12:0.00}", salesNet));
            sb.AppendLine();
            sb.AppendLine(string.Format("INPUT TAX CREDIT (PURCHASES - {0} GRNs)", purchases.Count));
            sb.AppendLine(string.Format("  Taxable Gross Value: Rs {0,12:0.00}", purGross));
            sb.AppendLine(string.Format("  Input SGST         : Rs {0,12:0.00}", purSgst));
            sb.AppendLine(string.Format("  Input CGST         : Rs {0,12:0.00}", purCgst));
            sb.AppendLine(string.Format("  Total Purchase Net : Rs {0,12:0.00}", purNet));
            sb.AppendLine();
            sb.AppendLine("----------------------------------------------------------");
            sb.AppendLine(string.Format("  NET GST PAYABLE (Output - Input): Rs {0,12:0.00}", (salesSgst + salesCgst) - (purSgst + purCgst)));
            sb.AppendLine("==========================================================");
            _lblGstSummary.Text = sb.ToString();
        }

        private void LoadStockReport()
        {
            int months = 0;
            if (_cmbExpiryFilter.SelectedIndex == 1) months = 3;
            else if (_cmbExpiryFilter.SelectedIndex == 2) months = 6;
            else if (_cmbExpiryFilter.SelectedIndex == 3) months = 12;

            var rows = _repo.GetStockAndExpiryReport(months);
            _stockGrid.DataSource = rows;
        }

        private static void ExportGridToCsv(DataGridView grid, string defaultName)
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = defaultName })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                var headers = new List<string>();
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    headers.Add("\"" + col.HeaderText.Replace("\"", "\"\"") + "\"");
                }
                sb.AppendLine(string.Join(",", headers.ToArray()));

                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cells = new List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string val = Convert.ToString(cell.Value ?? string.Empty);
                        cells.Add("\"" + val.Replace("\"", "\"\"") + "\"");
                    }
                    sb.AppendLine(string.Join(",", cells.ToArray()));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Exported successfully to:\n" + sfd.FileName, "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
