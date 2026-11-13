using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class ProductMasterForm : Form
    {
        private readonly PharmaRepository _repo;
        private TextBox _txtSearch;
        private DataGridView _grid;
        private List<Product> _products;

        public event Action DataChanged;

        public ProductMasterForm(PharmaRepository repo)
        {
            _repo = repo;
            InitializeComponent();
            RefreshGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Product & Stock Master";
            this.Size = new Size(960, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 244, 249);

            var topBar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 8, 10, 8) };
            var lblSearch = new Label { Text = "Search Product:", Left = 10, Top = 13, Width = 105, Font = new Font("Tahoma", 9F, FontStyle.Bold) };
            _txtSearch = new TextBox { Left = 120, Top = 10, Width = 320 };
            _txtSearch.TextChanged += delegate { RefreshGrid(); };

            var btnNew = new Button { Text = "New Product (Ctrl+N)", Left = 455, Top = 8, Width = 155, Height = 26 };
            btnNew.Click += delegate { OpenNewProduct(); };

            var btnEdit = new Button { Text = "Edit Selected", Left = 618, Top = 8, Width = 110, Height = 26 };
            btnEdit.Click += delegate { EditSelected(); };

            var btnBatches = new Button { Text = "Batches (F4)", Left = 736, Top = 8, Width = 105, Height = 26 };
            btnBatches.Click += delegate { ViewSelectedBatches(); };

            topBar.Controls.AddRange(new Control[] { lblSearch, _txtSearch, btnNew, btnEdit, btnBatches });

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Code", FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", FillWeight = 32 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PurchaseUnit", HeaderText = "Pur.Unit", FillWeight = 9 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PurchasePrice", HeaderText = "P.Price", FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleUnit", HeaderText = "Sale Unit", FillWeight = 9 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SalePrice", HeaderText = "Sale Price", FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Mrp", HeaderText = "MRP", FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HsnCode", HeaderText = "HSN", FillWeight = 11 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TaxPercent", HeaderText = "Tax%", FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalStockUnits", HeaderText = "Stock (Units)", FillWeight = 11, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##", Alignment = DataGridViewContentAlignment.MiddleRight } });

            _grid.CellDoubleClick += delegate { EditSelected(); };

            this.Controls.Add(_grid);
            this.Controls.Add(topBar);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.N)
                {
                    e.Handled = true;
                    OpenNewProduct();
                }
                else if (e.KeyCode == Keys.F4)
                {
                    e.Handled = true;
                    ViewSelectedBatches();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            };
        }

        private void RefreshGrid()
        {
            _products = _repo.GetAllProducts(_txtSearch.Text);
            _grid.DataSource = _products;
        }

        private Product GetSelectedProduct()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is Product)
            {
                return (Product)_grid.CurrentRow.DataBoundItem;
            }
            return null;
        }

        private void OpenNewProduct()
        {
            using (var dlg = new NewProductForm(_repo))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshGrid();
                    if (DataChanged != null) DataChanged();
                }
            }
        }

        private void EditSelected()
        {
            var prod = GetSelectedProduct();
            if (prod == null) return;
            using (var dlg = new NewProductForm(_repo, prod))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshGrid();
                    if (DataChanged != null) DataChanged();
                }
            }
        }

        private void ViewSelectedBatches()
        {
            var prod = GetSelectedProduct();
            if (prod == null) return;
            var batches = _repo.GetBatchesForProduct(prod.ProductCode);

            using (var form = new Form())
            {
                form.Text = string.Format("Batches - {0} ({1})", prod.ProductName, prod.ProductCode);
                form.Size = new Size(640, 320);
                form.StartPosition = FormStartPosition.CenterParent;

                var batchGrid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    DataSource = batches
                };
                form.Controls.Add(batchGrid);
                form.ShowDialog(this);
            }
        }
    }
}
