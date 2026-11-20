using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class NewProductForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly bool _isEditMode;
        private readonly Product _editingProduct;

        private TextBox _txtProductCode;
        private TextBox _txtProductName;
        private TextBox _txtPurchaseUnit;
        private TextBox _txtPurchasePrice;
        private TextBox _txtSaleUnit;
        private TextBox _txtSalePrice;
        private TextBox _txtMrp;
        private TextBox _txtHsnCode;
        private ComboBox _cmbTax;
        private Button _btnClose;
        private Button _btnOk;

        private bool _saleUnitManuallyChanged;
        private bool _salePriceManuallyChanged;
        private bool _suppressAutoSync;

        public Product SavedProduct { get; private set; }

        public NewProductForm(PharmaRepository repo, Product existingProduct = null, string initialName = "")
        {
            _repo = repo;
            _editingProduct = existingProduct;
            _isEditMode = existingProduct != null;

            InitializeComponent();
            LoadProductValues(initialName);
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "Edit Product" : "NewProduct";
            this.Size = new Size(440, 345);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.BackColor = Color.FromArgb(236, 240, 245);
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);

            var table = new TableLayoutPanel
            {
                Location = new Point(10, 10),
                Size = new Size(404, 248),
                ColumnCount = 2,
                RowCount = 9,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.White
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (int i = 0; i < 9; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            }

            _txtProductCode = CreateGridTextBox(readOnly: true, rightAlign: true);
            _txtProductCode.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _txtProductCode.BackColor = Color.FromArgb(245, 247, 250);

            _txtProductName = CreateGridTextBox(readOnly: false, rightAlign: false);
            _txtProductName.CharacterCasing = CharacterCasing.Upper;

            // Red border highlight around Product Name just like Shopaid.Net-RE
            var nameBorderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = Color.Red,
                Margin = new Padding(0)
            };
            _txtProductName.Dock = DockStyle.Fill;
            _txtProductName.BorderStyle = BorderStyle.None;
            nameBorderPanel.Controls.Add(_txtProductName);

            _txtPurchaseUnit = CreateGridTextBox(readOnly: false, rightAlign: true);
            _txtPurchasePrice = CreateGridTextBox(readOnly: false, rightAlign: true);
            _txtSaleUnit = CreateGridTextBox(readOnly: false, rightAlign: true);
            _txtSalePrice = CreateGridTextBox(readOnly: false, rightAlign: true);
            _txtMrp = CreateGridTextBox(readOnly: false, rightAlign: true);
            _txtHsnCode = CreateGridTextBox(readOnly: false, rightAlign: false);

            _cmbTax = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Font = new Font("Tahoma", 9F, FontStyle.Regular)
            };
            _cmbTax.Items.AddRange(new object[]
            {
                "0 %",
                "5 % GST",
                "12 % GST",
                "18 % GST",
                "28 % GST"
            });

            AddRow(table, 0, "Product Code", _txtProductCode);
            AddRow(table, 1, "Product Name", nameBorderPanel);
            AddRow(table, 2, "Purchase Unit", _txtPurchaseUnit);
            AddRow(table, 3, "Purchase Price", _txtPurchasePrice);
            AddRow(table, 4, "Sale Unit", _txtSaleUnit);
            AddRow(table, 5, "Sale Price", _txtSalePrice);
            AddRow(table, 6, "MRP", _txtMrp);
            AddRow(table, 7, "HSN Code", _txtHsnCode);
            AddRow(table, 8, "Tax", _cmbTax);

            _btnClose = new Button
            {
                Text = "Close",
                Size = new Size(92, 28),
                Location = new Point(220, 268),
                DialogResult = DialogResult.Cancel
            };
            _btnClose.Click += delegate { this.Close(); };

            _btnOk = new Button
            {
                Text = "&Ok",
                Size = new Size(92, 28),
                Location = new Point(322, 268),
                BackColor = Color.FromArgb(225, 234, 247)
            };
            _btnOk.Click += BtnOk_Click;

            this.Controls.Add(table);
            this.Controls.Add(_btnClose);
            this.Controls.Add(_btnOk);

            WireEvents();
        }

        private static TextBox CreateGridTextBox(bool readOnly, bool rightAlign)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ReadOnly = readOnly,
                TextAlign = rightAlign ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Padding(3, 4, 3, 2),
                Font = new Font("Tahoma", 9F, FontStyle.Regular)
            };
        }

        private static void AddRow(TableLayoutPanel table, int rowIndex, string labelText, Control inputControl)
        {
            var lbl = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.FromArgb(236, 240, 246),
                Font = new Font("Tahoma", 9F, FontStyle.Bold),
                Padding = new Padding(0, 0, 6, 0),
                Margin = new Padding(0)
            };
            table.Controls.Add(lbl, 0, rowIndex);
            table.Controls.Add(inputControl, 1, rowIndex);
        }

        private void WireEvents()
        {
            _txtPurchaseUnit.TextChanged += delegate
            {
                if (_suppressAutoSync || _saleUnitManuallyChanged) return;
                _suppressAutoSync = true;
                _txtSaleUnit.Text = _txtPurchaseUnit.Text;
                _suppressAutoSync = false;
            };

            _txtSaleUnit.TextChanged += delegate
            {
                if (_suppressAutoSync) return;
                _saleUnitManuallyChanged = true;
            };

            _txtMrp.TextChanged += delegate
            {
                if (_suppressAutoSync || _salePriceManuallyChanged) return;
                _suppressAutoSync = true;
                _txtSalePrice.Text = _txtMrp.Text;
                _suppressAutoSync = false;
            };

            _txtSalePrice.TextChanged += delegate
            {
                if (_suppressAutoSync) return;
                _salePriceManuallyChanged = true;
            };

            // Keyboard Enter navigation order:
            // Name -> Purchase Unit -> Purchase Price -> Sale Unit -> Sale Price -> MRP -> HSN Code -> Tax -> Ok
            AttachEnterAdvance(_txtProductName, _txtPurchaseUnit);
            AttachEnterAdvance(_txtPurchaseUnit, _txtPurchasePrice);
            AttachEnterAdvance(_txtPurchasePrice, _txtSaleUnit);
            AttachEnterAdvance(_txtSaleUnit, _txtSalePrice);
            AttachEnterAdvance(_txtSalePrice, _txtMrp);
            AttachEnterAdvance(_txtMrp, _txtHsnCode);

            _txtHsnCode.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Down || (e.KeyCode == Keys.Enter && _txtHsnCode.Text.Trim() == "0"))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    OpenHsnPopup();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    _cmbTax.Focus();
                    _cmbTax.SelectAll();
                }
            };

            _cmbTax.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    _btnOk.PerformClick();
                }
            };

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            this.Shown += delegate
            {
                _txtProductName.Focus();
                _txtProductName.SelectAll();
            };
        }

        private static void AttachEnterAdvance(Control current, Control next)
        {
            current.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    next.Focus();
                    if (next is TextBox)
                    {
                        ((TextBox)next).SelectAll();
                    }
                }
            };
        }

        private void OpenHsnPopup()
        {
            using (var popup = new HsnSelectorForm(_repo))
            {
                if (popup.ShowDialog(this) == DialogResult.OK && popup.SelectedHsn != null)
                {
                    _txtHsnCode.Text = popup.SelectedHsn.HsnCode;
                    SelectTaxSlab(popup.SelectedHsn.TaxPercent);
                    _cmbTax.Focus();
                }
            }
        }

        private void LoadProductValues(string initialName)
        {
            _suppressAutoSync = true;
            if (_isEditMode)
            {
                _txtProductCode.Text = _editingProduct.ProductCode.ToString();
                _txtProductName.Text = _editingProduct.ProductName;
                _txtPurchaseUnit.Text = _editingProduct.PurchaseUnit.ToString();
                _txtPurchasePrice.Text = _editingProduct.PurchasePrice.ToString("0.00", CultureInfo.InvariantCulture);
                _txtSaleUnit.Text = _editingProduct.SaleUnit.ToString();
                _txtSalePrice.Text = _editingProduct.SalePrice.ToString("0.00", CultureInfo.InvariantCulture);
                _txtMrp.Text = _editingProduct.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
                _txtHsnCode.Text = (_editingProduct.HsnCode == null || _editingProduct.HsnCode.Trim().Length == 0) ? "0" : _editingProduct.HsnCode;
                SelectTaxSlab(_editingProduct.TaxPercent);
                _saleUnitManuallyChanged = _editingProduct.SaleUnit != _editingProduct.PurchaseUnit;
                _salePriceManuallyChanged = _editingProduct.SalePrice != _editingProduct.Mrp;
            }
            else
            {
                _txtProductCode.Text = _repo.GetNextProductCode().ToString();
                _txtProductName.Text = initialName ?? string.Empty;
                _txtPurchaseUnit.Text = "1";
                _txtPurchasePrice.Text = "0.00";
                _txtSaleUnit.Text = "1";
                _txtSalePrice.Text = "0.00";
                _txtMrp.Text = "0.00";
                _txtHsnCode.Text = "0";
                SelectTaxSlab(5m);
            }
            _suppressAutoSync = false;
        }

        private void SelectTaxSlab(decimal taxPercent)
        {
            if (taxPercent == 0m) _cmbTax.SelectedIndex = 0;
            else if (taxPercent == 5m) _cmbTax.SelectedIndex = 1;
            else if (taxPercent == 12m) _cmbTax.SelectedIndex = 2;
            else if (taxPercent == 18m) _cmbTax.SelectedIndex = 3;
            else if (taxPercent == 28m) _cmbTax.SelectedIndex = 4;
            else
            {
                _cmbTax.SelectedIndex = -1;
                _cmbTax.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.##} % GST", taxPercent);
            }
        }

        private decimal GetSelectedTaxPercent()
        {
            string raw = (_cmbTax.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(raw))
            {
                var numChars = new System.Text.StringBuilder();
                bool dotSeen = false;
                for (int i = 0; i < raw.Length; i++)
                {
                    char c = raw[i];
                    if (char.IsDigit(c))
                    {
                        numChars.Append(c);
                    }
                    else if (c == '.' && !dotSeen && numChars.Length > 0)
                    {
                        dotSeen = true;
                        numChars.Append(c);
                    }
                    else if (numChars.Length > 0)
                    {
                        break;
                    }
                }

                decimal parsed;
                if (numChars.Length > 0 && decimal.TryParse(numChars.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                {
                    return parsed;
                }
            }

            switch (_cmbTax.SelectedIndex)
            {
                case 0: return 0m;
                case 1: return 5m;
                case 2: return 12m;
                case 3: return 18m;
                case 4: return 28m;
                default: return 5m;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string name = _txtProductName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter Product Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtProductName.Focus();
                return;
            }

            int purchaseUnit, saleUnit;
            decimal purchasePrice, salePrice, mrp;

            if (!int.TryParse(_txtPurchaseUnit.Text.Trim(), out purchaseUnit) || purchaseUnit <= 0)
            {
                MessageBox.Show("Purchase Unit must be a positive whole number (e.g. 1, 10, 15).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPurchaseUnit.Focus();
                return;
            }

            if (!int.TryParse(_txtSaleUnit.Text.Trim(), out saleUnit) || saleUnit <= 0)
            {
                saleUnit = purchaseUnit;
            }

            decimal.TryParse(_txtPurchasePrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out purchasePrice);
            decimal.TryParse(_txtMrp.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out mrp);
            if (!decimal.TryParse(_txtSalePrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out salePrice) || salePrice <= 0)
            {
                salePrice = mrp;
            }

            var product = _isEditMode ? _editingProduct : new Product();
            product.ProductCode = int.Parse(_txtProductCode.Text.Trim());
            product.ProductName = name;
            product.PurchaseUnit = purchaseUnit;
            product.PurchasePrice = purchasePrice;
            product.SaleUnit = saleUnit;
            product.SalePrice = salePrice;
            product.Mrp = mrp;
            product.HsnCode = (_txtHsnCode.Text == null || _txtHsnCode.Text.Trim().Length == 0) ? "0" : _txtHsnCode.Text.Trim();
            product.TaxPercent = GetSelectedTaxPercent();
            if (product.Rack == null || product.Rack.Trim().Length == 0) product.Rack = "STORE";

            if (_isEditMode)
            {
                _repo.UpdateProduct(product);
            }
            else
            {
                product.ProductCode = _repo.CreateProduct(product);
            }

            SavedProduct = product;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
