using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;
using PharmaAidPro.WinForms.Printing;

namespace PharmaAidPro.WinForms.Forms
{
    public class BillCopyForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;
        private TextBox _txtSearch;
        private DataGridView _grid;

        public BillCopyForm(PharmaRepository repo)
        {
            _repo = repo;
            _settings = _repo.GetStoreSettings();
            InitializeComponent();
            RefreshGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Bill Copy / Reprint (F8)";
            this.Size = new Size(860, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            var top = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(236, 240, 246), Padding = new Padding(8) };
            top.Controls.Add(new Label { Text = "Search Bill / Customer:", Left = 10, Top = 12, Width = 140, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _txtSearch = new TextBox { Left = 155, Top = 9, Width = 280 };
            _txtSearch.TextChanged += delegate { RefreshGrid(); };

            var btnPreview = new Button { Text = "Preview / Print Receipt", Left = 450, Top = 7, Width = 175, Height = 26 };
            btnPreview.Click += delegate { PreviewSelected(); };
            top.Controls.AddRange(new Control[] { _txtSearch, btnPreview });

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillNo", HeaderText = "Bill No", FillWeight = 16 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillDate", HeaderText = "Bill Date", FillWeight = 20 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Customer Name", FillWeight = 26 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentMode", HeaderText = "Mode", FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalItems", HeaderText = "Items", FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NetAmount", HeaderText = "Bill Amount", FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });

            _grid.CellDoubleClick += delegate { PreviewSelected(); };

            this.Controls.Add(_grid);
            this.Controls.Add(top);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) this.Close();
                else if (e.KeyCode == Keys.Enter) PreviewSelected();
            };
        }

        private void RefreshGrid()
        {
            _grid.DataSource = _repo.GetRecentSales(_txtSearch.Text);
        }

        private void PreviewSelected()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is SalesHeader)
            {
                var header = (SalesHeader)_grid.CurrentRow.DataBoundItem;
                List<SalesLine> lines = _repo.GetSalesLines(header.BillNo);
                DotMatrixReceiptPrinter.ShowPreviewDialog(this, _settings, header, lines);
            }
        }
    }
}
