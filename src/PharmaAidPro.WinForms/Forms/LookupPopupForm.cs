using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class SupplierLookupForm : Form
    {
        private readonly PharmaRepository _repo;
        private TextBox _txtSearch;
        private DataGridView _grid;
        public Supplier SelectedSupplier { get; private set; }

        public SupplierLookupForm(PharmaRepository repo, string initialFilter = "")
        {
            _repo = repo;
            this.Text = "Select Supplier";
            this.Size = new Size(680, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            var top = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(236, 240, 246), Padding = new Padding(6) };
            top.Controls.Add(new Label { Text = "Supplier Name:", Left = 8, Top = 10, Width = 96, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _txtSearch = new TextBox { Left = 108, Top = 7, Width = 340, Text = initialFilter ?? string.Empty };
            _txtSearch.TextChanged += delegate { RefreshData(); };
            _txtSearch.KeyDown += Search_KeyDown;
            top.Controls.Add(_txtSearch);

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
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierCode", HeaderText = "SupplierCode", FillWeight = 16 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "SupplierName", FillWeight = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Gstin", HeaderText = "TinNumber / GSTIN", FillWeight = 34 });

            _grid.CellDoubleClick += delegate { Confirm(); };
            _grid.KeyDown += Grid_KeyDown;

            this.Controls.Add(_grid);
            this.Controls.Add(top);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            RefreshData();
            this.Shown += delegate
            {
                _txtSearch.Focus();
                _txtSearch.SelectionStart = _txtSearch.Text.Length;
            };
        }

        private void RefreshData()
        {
            _grid.DataSource = _repo.SearchSuppliers(_txtSearch.Text);
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _grid.Rows.Count > 0)
            {
                e.Handled = true;
                _grid.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Confirm();
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Confirm();
            }
        }

        private void Confirm()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is Supplier)
            {
                SelectedSupplier = (Supplier)_grid.CurrentRow.DataBoundItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    public class ProductLookupForm : Form
    {
        private readonly PharmaRepository _repo;
        private TextBox _txtSearch;
        private DataGridView _grid;
        public Product SelectedProduct { get; private set; }

        public ProductLookupForm(PharmaRepository repo, string initialFilter = "")
        {
            _repo = repo;
            this.Text = "Select Product (Ctrl+N: New Product)";
            this.Size = new Size(760, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            var top = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(236, 240, 246), Padding = new Padding(6) };
            top.Controls.Add(new Label { Text = "Product Name:", Left = 8, Top = 11, Width = 96, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _txtSearch = new TextBox { Left = 108, Top = 8, Width = 360, Text = initialFilter ?? string.Empty };
            _txtSearch.TextChanged += delegate { RefreshData(); };
            _txtSearch.KeyDown += Search_KeyDown;

            var btnNew = new Button { Text = "New Product (Ctrl+N)", Left = 480, Top = 6, Width = 155, Height = 26, BackColor = Color.FromArgb(220, 232, 248) };
            btnNew.Click += delegate { OpenNewProduct(); };
            top.Controls.AddRange(new Control[] { _txtSearch, btnNew });

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
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "P_code", FillWeight = 14 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "ProductName", FillWeight = 42 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalStockUnits", HeaderText = "Stock", FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleUnit", HeaderText = "Pack", FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Mrp", HeaderText = "MRP", FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });

            _grid.CellDoubleClick += delegate { Confirm(); };
            _grid.KeyDown += Grid_KeyDown;

            this.Controls.Add(_grid);
            this.Controls.Add(top);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.N)
                {
                    e.Handled = true;
                    OpenNewProduct();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            RefreshData();
            this.Shown += delegate
            {
                _txtSearch.Focus();
                _txtSearch.SelectionStart = _txtSearch.Text.Length;
            };
        }

        private void RefreshData()
        {
            _grid.DataSource = _repo.SearchProducts(_txtSearch.Text);
        }

        private void OpenNewProduct()
        {
            using (var dlg = new NewProductForm(_repo, null, _txtSearch.Text.Trim()))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SavedProduct != null)
                {
                    SelectedProduct = dlg.SavedProduct;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _grid.Rows.Count > 0)
            {
                e.Handled = true;
                _grid.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Confirm();
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Confirm();
            }
        }

        private void Confirm()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is Product)
            {
                SelectedProduct = (Product)_grid.CurrentRow.DataBoundItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    public class BatchLookupForm : Form
    {
        private readonly DataGridView _grid;
        public ProductBatch SelectedBatch { get; private set; }

        public BatchLookupForm(List<ProductBatch> batches, string productName)
        {
            this.Text = "Batch List - " + productName;
            this.Size = new Size(600, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

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
                BackgroundColor = Color.White,
                DataSource = batches
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BatchDesc", HeaderText = "Batch Desc", FillWeight = 24 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ExpiryMMYY", HeaderText = "Expiry", FillWeight = 16 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StockBaseUnits", HeaderText = "Stock (Units)", FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PPrice", HeaderText = "PPrice", FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Mrp", HeaderText = "MRP", FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });

            _grid.CellDoubleClick += delegate { Confirm(); };
            _grid.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    Confirm();
                }
            };

            this.Controls.Add(_grid);
            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }

        private void Confirm()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is ProductBatch)
            {
                SelectedBatch = (ProductBatch)_grid.CurrentRow.DataBoundItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
