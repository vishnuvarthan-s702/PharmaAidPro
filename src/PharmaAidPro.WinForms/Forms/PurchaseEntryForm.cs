using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class PurchaseEntryForm : Form
    {
        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;
        private Supplier _selectedSupplier;

        // Header controls
        private TextBox _txtGrnDate;
        private Label _lblPossibleGrn;
        private TextBox _txtSupplierName;
        private Label _lblSupplierCode;
        private Label _lblSupplierAddr;
        private Label _lblSupplierPhone;
        private Label _lblSupplierGst;
        private Label _lblOutstanding;
        private TextBox _txtInvoiceNo;
        private TextBox _txtInvoiceDate;
        private TextBox _txtPaymentDate;
        private TextBox _txtAmountPayable;
        private TextBox _txtDiscountPct;
        private TextBox _txtDiscountAmt;
        private ComboBox _cmbSeries;
        private TextBox _txtCoinAdj;

        // Grid
        private DataGridView _grid;

        // Bottom tax & margin controls
        private DataGridView _taxGrid;
        private Label _lblTotalTaxSummary;
        private Label _lblDiff;
        private Label _lblItemCost;
        private Label _lblCurMargin;
        private Label _lblBatchStock;
        private Label _lblTotalStock;
        private Label _lblMrpTotal;
        private Label _lblNetPayableSummary;

        private bool _suppressCalc;

        private InlineSuggestPanel<Product> _productDropdown;
        private InlineSuggestPanel<Supplier> _supplierDropdown;
        private TextBox _activeProductTextBox;
        private int _activeEditRowIndex = -1;
        private bool _suppressSuggest;

        public event Action GrnSaved;

        public PurchaseEntryForm(PharmaRepository repo)
        {
            _repo = repo;
            _settings = _repo.GetStoreSettings();
            InitializeComponent();
            ResetForm();
        }

        private void InitializeComponent()
        {
            this.Text = "Purchase Entry - [Purchase Invoice]";
            this.Size = new Size(1220, 680);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 8.5F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(236, 240, 246);

            // Top Header Panel
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 165, BackColor = Color.FromArgb(238, 242, 248), Padding = new Padding(8) };

            // Row 1: GRN Date & Possible GRN No
            headerPanel.Controls.Add(new Label { Text = "GRN Date", Left = 10, Top = 10, Width = 72 });
            _txtGrnDate = new TextBox { Left = 85, Top = 7, Width = 85 };
            FullDateMaskHelper.AttachPermanent(_txtGrnDate);
            headerPanel.Controls.Add(_txtGrnDate);

            headerPanel.Controls.Add(new Label { Text = "Possible GRN No", Left = 220, Top = 10, Width = 100 });
            _lblPossibleGrn = new Label { Left = 325, Top = 9, Width = 70, Font = new Font("Tahoma", 10F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            headerPanel.Controls.Add(_lblPossibleGrn);

            _lblSupplierGst = new Label { Left = 420, Top = 10, Width = 320, Font = new Font("Tahoma", 8.5F, FontStyle.Bold) };
            headerPanel.Controls.Add(_lblSupplierGst);

            // Left Supplier Box
            headerPanel.Controls.Add(new Label { Text = "Supplier Name", Left = 10, Top = 38, Width = 85 });
            _txtSupplierName = new TextBox { Left = 100, Top = 35, Width = 195, CharacterCasing = CharacterCasing.Upper };
            _txtSupplierName.TextChanged += TxtSupplierName_TextChanged;
            _txtSupplierName.KeyDown += TxtSupplierName_KeyDown;
            headerPanel.Controls.Add(_txtSupplierName);

            headerPanel.Controls.Add(new Label { Text = "Code", Left = 10, Top = 64, Width = 60 });
            _lblSupplierCode = new Label { Left = 100, Top = 64, Width = 100, Font = new Font("Tahoma", 8.5F, FontStyle.Bold) };
            headerPanel.Controls.Add(_lblSupplierCode);

            _lblSupplierAddr = new Label { Left = 10, Top = 88, Width = 285, Height = 32 };
            headerPanel.Controls.Add(_lblSupplierAddr);

            headerPanel.Controls.Add(new Label { Text = "Phone No", Left = 10, Top = 128, Width = 70 });
            _lblSupplierPhone = new Label { Left = 100, Top = 128, Width = 180 };
            headerPanel.Controls.Add(_lblSupplierPhone);

            // Middle Invoice Details
            _lblOutstanding = new Label { Left = 330, Top = 35, Width = 260, Height = 20, Font = new Font("Tahoma", 9.5F, FontStyle.Bold), ForeColor = Color.DarkRed };
            headerPanel.Controls.Add(_lblOutstanding);

            headerPanel.Controls.Add(new Label { Text = "Invoice No", Left = 330, Top = 64, Width = 95 });
            _txtInvoiceNo = new TextBox { Left = 430, Top = 61, Width = 130, CharacterCasing = CharacterCasing.Upper };
            headerPanel.Controls.Add(_txtInvoiceNo);

            headerPanel.Controls.Add(new Label { Text = "Invoice Date", Left = 330, Top = 89, Width = 95 });
            _txtInvoiceDate = new TextBox { Left = 430, Top = 86, Width = 130 };
            FullDateMaskHelper.AttachPermanent(_txtInvoiceDate);
            _txtInvoiceDate.TextChanged += delegate
            {
                if (_txtPaymentDate != null)
                {
                    _txtPaymentDate.Text = _txtInvoiceDate.Text;
                }
            };
            headerPanel.Controls.Add(_txtInvoiceDate);

            headerPanel.Controls.Add(new Label { Text = "Payment Date", Left = 330, Top = 114, Width = 95 });
            _txtPaymentDate = new TextBox { Left = 430, Top = 111, Width = 130 };
            FullDateMaskHelper.AttachPermanent(_txtPaymentDate);
            headerPanel.Controls.Add(_txtPaymentDate);

            headerPanel.Controls.Add(new Label { Text = "Amount Payable", Left = 330, Top = 139, Width = 95, Font = new Font("Tahoma", 8.5F, FontStyle.Bold) });
            _txtAmountPayable = new TextBox { Left = 430, Top = 136, Width = 130, TextAlign = HorizontalAlignment.Right, Text = "0.00" };
            _txtAmountPayable.TextChanged += delegate { RecalculateAll(); };
            headerPanel.Controls.Add(_txtAmountPayable);

            // Right Discount & Series
            headerPanel.Controls.Add(new Label { Text = "Discount %", Left = 590, Top = 64, Width = 85 });
            _txtDiscountPct = new TextBox { Left = 680, Top = 61, Width = 110, TextAlign = HorizontalAlignment.Right, Text = "0.0000" };
            _txtDiscountPct.TextChanged += delegate
            {
                if (_suppressCalc) return;
                _suppressCalc = true;
                decimal gross = CalculateGross();
                decimal pct = ParseDec(_txtDiscountPct.Text);
                _txtDiscountAmt.Text = (gross * pct / 100m).ToString("0.00", CultureInfo.InvariantCulture);
                _suppressCalc = false;
                RecalculateAll();
            };
            headerPanel.Controls.Add(_txtDiscountPct);

            headerPanel.Controls.Add(new Label { Text = "Discount Amt", Left = 590, Top = 89, Width = 85 });
            _txtDiscountAmt = new TextBox { Left = 680, Top = 86, Width = 110, TextAlign = HorizontalAlignment.Right, Text = "0.00" };
            _txtDiscountAmt.TextChanged += delegate
            {
                if (_suppressCalc) return;
                _suppressCalc = true;
                decimal gross = CalculateGross();
                decimal amt = ParseDec(_txtDiscountAmt.Text);
                _txtDiscountPct.Text = gross > 0 ? (amt * 100m / gross).ToString("0.0000", CultureInfo.InvariantCulture) : "0.0000";
                _suppressCalc = false;
                RecalculateAll();
            };
            headerPanel.Controls.Add(_txtDiscountAmt);

            headerPanel.Controls.Add(new Label { Text = "Series", Left = 590, Top = 114, Width = 85 });
            _cmbSeries = new ComboBox { Left = 680, Top = 111, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbSeries.Items.AddRange(new object[] { "INVOICE CREDIT", "INVOICE CASH" });
            _cmbSeries.SelectedIndex = 0;
            headerPanel.Controls.Add(_cmbSeries);

            headerPanel.Controls.Add(new Label { Text = "Coin Adj.", Left = 590, Top = 139, Width = 85 });
            _txtCoinAdj = new TextBox { Left = 680, Top = 136, Width = 110, TextAlign = HorizontalAlignment.Right, Text = "0.00" };
            _txtCoinAdj.TextChanged += delegate { RecalculateAll(); };
            AttachAutoReplaceOnClickAndFocus(_txtCoinAdj);
            AttachAutoReplaceOnClickAndFocus(_txtAmountPayable);
            AttachAutoReplaceOnClickAndFocus(_txtDiscountPct);
            AttachAutoReplaceOnClickAndFocus(_txtDiscountAmt);
            headerPanel.Controls.Add(_txtCoinAdj);

            var btnNewProdTop = new Button
            {
                Text = "New Product (Ctrl+N)",
                Left = 860,
                Top = 34,
                Width = 155,
                Height = 30,
                BackColor = Color.FromArgb(220, 232, 248)
            };
            btnNewProdTop.Click += delegate { OpenNewProductDialog(string.Empty); };
            headerPanel.Controls.Add(btnNewProdTop);

            // Grid
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EditMode = DataGridViewEditMode.EditOnEnter
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sno", HeaderText = "Sno", ReadOnly = true, FillWeight = 4 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PCode", HeaderText = "PCode", ReadOnly = true, FillWeight = 7 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Product Name", FillWeight = 24 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FreeQty", HeaderText = "Free Qty", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiscAmt", HeaderText = "Disc Amt", FillWeight = 7, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProdDisPct", HeaderText = "Prod Dis%", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchDesc", HeaderText = "Batch Desc", FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expiry", HeaderText = "Expiry", FillWeight = 7 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hsn", HeaderText = "HSN", FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TaxPct", HeaderText = "Tax%", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PPrice", HeaderText = "PPrice", FillWeight = 8, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SPrice", HeaderText = "SPrice", FillWeight = 8, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mrp", HeaderText = "MRP", FillWeight = 8, DefaultCellStyle = RightStyle() });

            _grid.RowsAdded += delegate { RenumberRows(); };
            _grid.CellBeginEdit += Grid_CellBeginEdit;
            _grid.EditingControlShowing += Grid_EditingControlShowing;
            _grid.CellEndEdit += Grid_CellEndEdit;
            _grid.SelectionChanged += delegate { UpdateSelectedRowMarginPanel(); };
            _grid.KeyDown += Grid_KeyDown;

            _productDropdown = new InlineSuggestPanel<Product>(
                "Up/Down: Navigate  |  Enter: Select Product  |  Ctrl+N: New Product  |  Esc: Close",
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "P_code", FillWeight = 13 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", FillWeight = 44 },
                new DataGridViewTextBoxColumn { DataPropertyName = "TotalStockUnits", HeaderText = "Stock", FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { DataPropertyName = "PurchaseUnit", HeaderText = "Pack", FillWeight = 11, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { DataPropertyName = "Mrp", HeaderText = "MRP", FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _productDropdown.ItemChosen += ProductDropdown_ItemChosen;

            _supplierDropdown = new InlineSuggestPanel<Supplier>(
                "Up/Down: Navigate  |  Enter: Select Supplier  |  Esc: Close",
                new DataGridViewTextBoxColumn { DataPropertyName = "SupplierCode", HeaderText = "Code", FillWeight = 16 },
                new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier Name", FillWeight = 52 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Gstin", HeaderText = "GSTIN", FillWeight = 32 });
            _supplierDropdown.ItemChosen += SupplierDropdown_ItemChosen;

            // Bottom Panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 155, BackColor = Color.FromArgb(236, 240, 246), Padding = new Padding(8) };

            _taxGrid = new DataGridView
            {
                Left = 10,
                Top = 8,
                Width = 480,
                Height = 96,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _taxGrid.Columns.Add("TaxSlab", "Tax %");
            _taxGrid.Columns.Add("Gross", "Gross Amt");
            _taxGrid.Columns.Add("SGST", "SGST");
            _taxGrid.Columns.Add("CGST", "CGST");
            _taxGrid.Columns.Add("Net", "Net");
            bottomPanel.Controls.Add(_taxGrid);

            _lblTotalTaxSummary = new Label { Left = 10, Top = 110, Width = 480, Height = 20, Font = new Font("Tahoma", 8.5F, FontStyle.Bold) };
            bottomPanel.Controls.Add(_lblTotalTaxSummary);

            // Middle Stock & Margin Info
            _lblBatchStock = new Label { Left = 510, Top = 10, Width = 180, Text = "Batch Stock : 0" };
            _lblTotalStock = new Label { Left = 510, Top = 32, Width = 180, Text = "Total Stock : 0" };
            _lblItemCost = new Label { Left = 510, Top = 54, Width = 180, Text = "Item Cost : 0.00" };
            _lblCurMargin = new Label { Left = 510, Top = 76, Width = 180, Text = "Cur.Margin : 0.00 %" };
            bottomPanel.Controls.AddRange(new Control[] { _lblBatchStock, _lblTotalStock, _lblItemCost, _lblCurMargin });

            // Right Totals & Process Buttons
            _lblMrpTotal = new Label { Left = 720, Top = 10, Width = 250, Font = new Font("Tahoma", 9F, FontStyle.Bold), Text = "Mrp Total : 0.00" };
            _lblNetPayableSummary = new Label { Left = 720, Top = 34, Width = 250, Font = new Font("Tahoma", 10F, FontStyle.Bold), ForeColor = Color.DarkBlue, Text = "Calculated Net : 0.00" };
            _lblDiff = new Label { Left = 720, Top = 58, Width = 250, Font = new Font("Tahoma", 9.5F, FontStyle.Bold), ForeColor = Color.DarkRed, Text = "Diff : 0.00" };
            bottomPanel.Controls.AddRange(new Control[] { _lblMrpTotal, _lblNetPayableSummary, _lblDiff });

            var btnCancel = new Button { Text = "Cancel", Left = 720, Top = 96, Width = 105, Height = 34 };
            btnCancel.Click += delegate { this.Close(); };

            var btnProcess = new Button { Text = "Process - F1", Left = 835, Top = 96, Width = 135, Height = 34, BackColor = Color.FromArgb(210, 228, 250), Font = new Font("Tahoma", 9F, FontStyle.Bold) };
            btnProcess.Click += delegate { ProcessAndSaveGrn(); };
            bottomPanel.Controls.AddRange(new Control[] { btnCancel, btnProcess });

            this.Controls.Add(_grid);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(_productDropdown);
            this.Controls.Add(_supplierDropdown);

            // Enter navigation across header controls
            AttachEnter(_txtGrnDate, _txtSupplierName);
            AttachEnter(_txtInvoiceNo, _txtInvoiceDate);
            _txtInvoiceDate.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    _txtInvoiceDate.Text = FullDateMaskHelper.NormalizeDate(_txtInvoiceDate.Text);
                    _txtPaymentDate.Text = _txtInvoiceDate.Text;
                    _txtPaymentDate.Focus();
                    FullDateMaskHelper.SelectFirstSegment(_txtPaymentDate);
                }
            };
            AttachEnter(_txtPaymentDate, _txtAmountPayable);
            _txtAmountPayable.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (!EnsureAmountPayableFilled())
                    {
                        return;
                    }
                    _txtDiscountPct.Focus();
                    _txtDiscountPct.SelectAll();
                }
            };
            _txtDiscountPct.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (!EnsureAmountPayableFilled())
                    {
                        return;
                    }
                    _grid.Focus();
                    if (_grid.Rows.Count > 0 && _grid.Columns.Contains("ProductName"))
                    {
                        _grid.CurrentCell = _grid.Rows[0].Cells["ProductName"];
                        _grid.BeginEdit(true);
                    }
                }
            };

            AttachEnter(_txtDiscountAmt, _cmbSeries);
            AttachEnter(_cmbSeries, _txtCoinAdj);
            _txtCoinAdj.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    FocusProductGrid(0, "ProductName");
                }
            };

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.F1)
                {
                    e.Handled = true;
                    ProcessAndSaveGrn();
                }
                else if (e.Control && e.KeyCode == Keys.N)
                {
                    e.Handled = true;
                    OpenNewProductDialog(string.Empty);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            };
        }

        private static DataGridViewCellStyle RightStyle()
        {
            return new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
        }

        private static void AttachAutoReplaceOnClickAndFocus(TextBox tb)
        {
            if (tb == null) return;
            bool replaceOnNextChar = false;

            tb.Enter += delegate
            {
                replaceOnNextChar = true;
                tb.BeginInvoke((MethodInvoker)delegate
                {
                    if (!tb.IsDisposed) tb.SelectAll();
                });
            };

            tb.MouseUp += delegate
            {
                tb.SelectAll();
                replaceOnNextChar = true;
            };

            tb.KeyPress += delegate(object s, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && replaceOnNextChar)
                {
                    replaceOnNextChar = false;
                    tb.Text = e.KeyChar.ToString();
                    tb.SelectionStart = tb.Text.Length;
                    tb.SelectionLength = 0;
                    e.Handled = true;
                }
            };

            tb.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
                {
                    replaceOnNextChar = false;
                }
                else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                {
                    if (replaceOnNextChar)
                    {
                        replaceOnNextChar = false;
                        tb.Text = string.Empty;
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
            };
        }

        private static void AttachEnter(Control current, Control next)
        {
            current.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    next.Focus();
                }
            };
        }

        private void ResetForm()
        {
            string today = DateTime.Now.ToString("dd/MM/yy");
            _txtGrnDate.Text = today;
            _txtInvoiceDate.Text = today;
            _txtPaymentDate.Text = today;
            _lblPossibleGrn.Text = _repo.GetNextGrnNumber().ToString();
            _selectedSupplier = null;
            _txtSupplierName.Text = string.Empty;
            _lblSupplierCode.Text = string.Empty;
            _lblSupplierAddr.Text = string.Empty;
            _lblSupplierPhone.Text = string.Empty;
            _lblSupplierGst.Text = string.Empty;
            _lblOutstanding.Text = string.Empty;
            _txtInvoiceNo.Text = string.Empty;
            _txtAmountPayable.Text = "0.00";
            _txtDiscountPct.Text = "0.0000";
            _txtDiscountAmt.Text = "0.00";
            _txtCoinAdj.Text = "0.00";
            _grid.Rows.Clear();
            RecalculateAll();
        }

        private void TxtSupplierName_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSuggest) return;
            string q = _txtSupplierName.Text.Trim();
            if (string.IsNullOrEmpty(q))
            {
                _supplierDropdown.HidePopup();
                return;
            }
            var screenPt = _txtSupplierName.Parent.PointToScreen(new Point(_txtSupplierName.Left, _txtSupplierName.Bottom + 2));
            var hostPt = this.PointToClient(screenPt);
            _supplierDropdown.ShowAt(this, hostPt, _repo.SearchSuppliers(q));
        }

        private void SupplierDropdown_ItemChosen(Supplier supplier)
        {
            ApplySelectedSupplier(supplier);
            _txtInvoiceNo.Focus();
        }

        private void ApplySelectedSupplier(Supplier supplier)
        {
            _suppressSuggest = true;
            _selectedSupplier = supplier;
            _txtSupplierName.Text = _selectedSupplier.SupplierName;
            _lblSupplierCode.Text = _selectedSupplier.SupplierCode.ToString();
            _lblSupplierAddr.Text = _selectedSupplier.Address;
            _lblSupplierPhone.Text = _selectedSupplier.Phone;
            _lblSupplierGst.Text = string.Format("GST NO : {0}  {1}", _selectedSupplier.Gstin, _selectedSupplier.StateName);
            _lblOutstanding.Text = string.Format("Outstanding Rs {0:0.00}", _selectedSupplier.OutstandingAmount);
            _suppressSuggest = false;
        }

        private void TxtSupplierName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && !_supplierDropdown.Visible)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                var screenPt = _txtSupplierName.Parent.PointToScreen(new Point(_txtSupplierName.Left, _txtSupplierName.Bottom + 2));
                var hostPt = this.PointToClient(screenPt);
                _supplierDropdown.ShowAt(this, hostPt, _repo.SearchSuppliers(_txtSupplierName.Text));
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                using (var dlg = new SupplierLookupForm(_repo, _txtSupplierName.Text))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedSupplier != null)
                    {
                        ApplySelectedSupplier(dlg.SelectedSupplier);
                        _txtInvoiceNo.Focus();
                    }
                }
            }
        }

        private bool EnsureAmountPayableFilled()
        {
            if (ParseDec(_txtAmountPayable.Text) <= 0m)
            {
                MessageBox.Show(
                    "Please enter the Amount Payable (below Payment Date) before entering products.",
                    "Amount Payable Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtAmountPayable.Focus();
                _txtAmountPayable.SelectAll();
                return false;
            }
            return true;
        }

        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (ParseDec(_txtAmountPayable.Text) <= 0m)
            {
                e.Cancel = true;
                BeginInvoke((MethodInvoker)delegate
                {
                    EnsureAmountPayableFilled();
                });
            }
        }

        private void Grid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_activeProductTextBox != null)
            {
                _activeProductTextBox.TextChanged -= ActiveProductTextBox_TextChanged;
                _activeProductTextBox = null;
            }
            ExpiryMaskHelper.Detach();

            if (_grid.CurrentCell != null && _grid.Columns[_grid.CurrentCell.ColumnIndex].Name == "ProductName")
            {
                var tb = e.Control as TextBox;
                if (tb != null)
                {
                    _activeProductTextBox = tb;
                    _activeEditRowIndex = _grid.CurrentCell.RowIndex;
                    _activeProductTextBox.TextChanged += ActiveProductTextBox_TextChanged;
                }
            }
            else if (_grid.CurrentCell != null && _grid.Columns[_grid.CurrentCell.ColumnIndex].Name == "Expiry")
            {
                _productDropdown.HidePopup();
                ExpiryMaskHelper.Attach(e.Control as TextBox);
            }
            else
            {
                _productDropdown.HidePopup();
            }
        }

        private void ActiveProductTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_suppressSuggest || _activeProductTextBox == null || _activeEditRowIndex < 0) return;
            string query = _activeProductTextBox.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                _productDropdown.HidePopup();
                return;
            }
            ShowProductDropdownBelowCell(_activeEditRowIndex, query);
        }

        private void ShowProductDropdownBelowCell(int rowIndex, string query)
        {
            var colIdx = _grid.Columns["ProductName"].Index;
            var rect = _grid.GetCellDisplayRectangle(colIdx, rowIndex, true);
            var screenPt = _grid.PointToScreen(new Point(rect.Left, rect.Bottom));
            var hostPt = this.PointToClient(screenPt);
            _productDropdown.ShowAt(this, hostPt, _repo.SearchProducts(query));
        }

        private void ProductDropdown_ItemChosen(Product prod)
        {
            int targetRow = _activeEditRowIndex >= 0 && _activeEditRowIndex < _grid.Rows.Count
                ? _activeEditRowIndex
                : (_grid.CurrentRow != null ? _grid.CurrentRow.Index : -1);
            if (targetRow < 0) return;

            _suppressSuggest = true;
            _grid.EndEdit();
            PopulateRowFromProduct(_grid.Rows[targetRow], prod);
            RecalculateAll();
            UpdateSelectedRowMarginPanel();
            _suppressSuggest = false;

            if (targetRow < _grid.Rows.Count)
            {
                _grid.CurrentCell = _grid.Rows[targetRow].Cells["Qty"];
                _grid.BeginEdit(true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_supplierDropdown != null && _supplierDropdown.Visible)
            {
                if (keyData == Keys.Down) { _supplierDropdown.MoveSelection(1); return true; }
                if (keyData == Keys.Up) { _supplierDropdown.MoveSelection(-1); return true; }
                if (keyData == Keys.Enter && _supplierDropdown.ConfirmCurrent()) { return true; }
                if (keyData == Keys.Escape) { _supplierDropdown.HidePopup(); return true; }
            }

            if (_productDropdown != null && _productDropdown.Visible)
            {
                if (keyData == Keys.Down) { _productDropdown.MoveSelection(1); return true; }
                if (keyData == Keys.Up) { _productDropdown.MoveSelection(-1); return true; }
                if (keyData == Keys.Enter && _productDropdown.ConfirmCurrent()) { return true; }
                if (keyData == Keys.Escape) { _productDropdown.HidePopup(); return true; }
            }

            if (_grid != null && _grid.CurrentCell != null && (_grid.Focused || _grid.ContainsFocus || _grid.IsCurrentCellInEditMode))
            {
                if (keyData == Keys.Enter)
                {
                    MoveToNextGridFieldInRow();
                    return true;
                }
                if (keyData == Keys.Delete)
                {
                    DeleteCurrentGridRow();
                    return true;
                }
                if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
                {
                    if (HandleGridArrowNavigation(keyData))
                    {
                        return true;
                    }
                }
            }
            else if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
            {
                if (HandleHeaderArrowNavigation(keyData))
                {
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static bool CanLeaveTextBoxLeft(TextBox tb)
        {
            if (tb == null) return true;
            if (tb.Text.Length == 0) return true;
            if (tb.SelectionLength == tb.Text.Length) return true;
            return tb.SelectionLength == 0 && tb.SelectionStart == 0;
        }

        private static bool CanLeaveTextBoxRight(TextBox tb)
        {
            if (tb == null) return true;
            if (tb.Text.Length == 0) return true;
            if (tb.SelectionLength == tb.Text.Length) return true;
            return tb.SelectionLength == 0 && tb.SelectionStart >= tb.Text.Length;
        }

        private void FocusHeaderControl(Control target)
        {
            if (target == null) return;
            target.Focus();
            var tb = target as TextBox;
            if (tb != null)
            {
                if (tb == _txtGrnDate || tb == _txtInvoiceDate || tb == _txtPaymentDate)
                {
                    FullDateMaskHelper.SelectFirstSegment(tb);
                }
                else
                {
                    tb.SelectAll();
                }
            }
        }

        private void FocusProductGrid(int rowIndex, string colName)
        {
            if (!EnsureAmountPayableFilled())
            {
                return;
            }
            _grid.Focus();
            if (_grid.Rows.Count == 0)
            {
                _grid.Rows.Add();
            }
            int safeRow = Math.Max(0, Math.Min(rowIndex, _grid.Rows.Count - 1));
            string targetCol = _grid.Columns.Contains(colName) ? colName : "ProductName";
            _grid.CurrentCell = _grid.Rows[safeRow].Cells[targetCol];
            _grid.BeginEdit(true);
        }

        private bool HandleHeaderArrowNavigation(Keys keyData)
        {
            Control active = this.ActiveControl;
            if (active == null) return false;

            if (active == _txtGrnDate)
            {
                if (keyData == Keys.Down) { _txtGrnDate.Text = FullDateMaskHelper.NormalizeDate(_txtGrnDate.Text); FocusHeaderControl(_txtSupplierName); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtGrnDate)) { _txtGrnDate.Text = FullDateMaskHelper.NormalizeDate(_txtGrnDate.Text); FocusHeaderControl(_txtInvoiceNo); return true; }
                return false;
            }

            if (active == _txtSupplierName)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_txtGrnDate); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtSupplierName)) { FocusHeaderControl(_txtInvoiceNo); return true; }
                if (keyData == Keys.Down)
                {
                    // If a supplier is already selected and matches, Down jumps directly to the Product Grid;
                    // otherwise let TxtSupplierName_KeyDown open the supplier dropdown.
                    if (_selectedSupplier != null &&
                        string.Equals(_txtSupplierName.Text.Trim(), _selectedSupplier.SupplierName, StringComparison.OrdinalIgnoreCase))
                    {
                        FocusProductGrid(0, "ProductName");
                        return true;
                    }
                    return false;
                }
                return false;
            }

            if (active == _txtInvoiceNo)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_txtGrnDate); return true; }
                if (keyData == Keys.Down) { FocusHeaderControl(_txtInvoiceDate); return true; }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtInvoiceNo)) { FocusHeaderControl(_txtSupplierName); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtInvoiceNo)) { FocusHeaderControl(_txtDiscountPct); return true; }
                return false;
            }

            if (active == _txtInvoiceDate)
            {
                if (keyData == Keys.Up)
                {
                    _txtInvoiceDate.Text = FullDateMaskHelper.NormalizeDate(_txtInvoiceDate.Text);
                    _txtPaymentDate.Text = _txtInvoiceDate.Text;
                    FocusHeaderControl(_txtInvoiceNo);
                    return true;
                }
                if (keyData == Keys.Down)
                {
                    _txtInvoiceDate.Text = FullDateMaskHelper.NormalizeDate(_txtInvoiceDate.Text);
                    _txtPaymentDate.Text = _txtInvoiceDate.Text;
                    FocusHeaderControl(_txtPaymentDate);
                    return true;
                }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtInvoiceDate)) { FocusHeaderControl(_txtSupplierName); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtInvoiceDate)) { FocusHeaderControl(_txtDiscountAmt); return true; }
                return false;
            }

            if (active == _txtPaymentDate)
            {
                if (keyData == Keys.Up)
                {
                    _txtPaymentDate.Text = FullDateMaskHelper.NormalizeDate(_txtPaymentDate.Text);
                    FocusHeaderControl(_txtInvoiceDate);
                    return true;
                }
                if (keyData == Keys.Down)
                {
                    _txtPaymentDate.Text = FullDateMaskHelper.NormalizeDate(_txtPaymentDate.Text);
                    FocusHeaderControl(_txtAmountPayable);
                    return true;
                }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtPaymentDate)) { FocusHeaderControl(_txtSupplierName); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtPaymentDate)) { FocusHeaderControl(_cmbSeries); return true; }
                return false;
            }

            if (active == _txtAmountPayable)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_txtPaymentDate); return true; }
                if (keyData == Keys.Down) { FocusProductGrid(0, "ProductName"); return true; }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtAmountPayable)) { FocusHeaderControl(_txtSupplierName); return true; }
                if (keyData == Keys.Right && CanLeaveTextBoxRight(_txtAmountPayable)) { FocusHeaderControl(_txtCoinAdj); return true; }
                return false;
            }

            if (active == _txtDiscountPct)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_txtInvoiceNo); return true; }
                if (keyData == Keys.Down) { FocusHeaderControl(_txtDiscountAmt); return true; }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtDiscountPct)) { FocusHeaderControl(_txtInvoiceNo); return true; }
                return false;
            }

            if (active == _txtDiscountAmt)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_txtDiscountPct); return true; }
                if (keyData == Keys.Down) { FocusHeaderControl(_cmbSeries); return true; }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtDiscountAmt)) { FocusHeaderControl(_txtInvoiceDate); return true; }
                return false;
            }

            if (active == _cmbSeries)
            {
                if (_cmbSeries.DroppedDown) return false;
                if (keyData == Keys.Left) { FocusHeaderControl(_txtPaymentDate); return true; }
                if (keyData == Keys.Right) { FocusHeaderControl(_txtCoinAdj); return true; }
                if (keyData == Keys.Up && _cmbSeries.SelectedIndex <= 0) { FocusHeaderControl(_txtDiscountAmt); return true; }
                if (keyData == Keys.Down && _cmbSeries.SelectedIndex >= _cmbSeries.Items.Count - 1) { FocusHeaderControl(_txtCoinAdj); return true; }
                return false;
            }

            if (active == _txtCoinAdj)
            {
                if (keyData == Keys.Up) { FocusHeaderControl(_cmbSeries); return true; }
                if (keyData == Keys.Down) { FocusProductGrid(0, "ProductName"); return true; }
                if (keyData == Keys.Left && CanLeaveTextBoxLeft(_txtCoinAdj)) { FocusHeaderControl(_txtAmountPayable); return true; }
                return false;
            }

            return false;
        }

        private bool HandleGridArrowNavigation(Keys keyData)
        {
            if (_grid == null || _grid.CurrentCell == null) return false;

            int rowIdx = _grid.CurrentCell.RowIndex;
            int colIdx = _grid.CurrentCell.ColumnIndex;
            var editTb = _grid.EditingControl as TextBox;

            if (keyData == Keys.Left)
            {
                if (editTb != null && !CanLeaveTextBoxLeft(editTb))
                {
                    return false; // Let caret move left inside the text
                }

                _grid.EndEdit();
                for (int c = colIdx - 1; c >= 0; c--)
                {
                    if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                    {
                        _grid.CurrentCell = _grid.Rows[rowIdx].Cells[c];
                        _grid.BeginEdit(true);
                        return true;
                    }
                }

                // At first editable column: move to previous row's last editable column, or up to Supplier Name from row 0
                if (rowIdx > 0)
                {
                    for (int c = _grid.Columns.Count - 1; c >= 0; c--)
                    {
                        if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                        {
                            _grid.CurrentCell = _grid.Rows[rowIdx - 1].Cells[c];
                            _grid.BeginEdit(true);
                            return true;
                        }
                    }
                }
                else
                {
                    FocusHeaderControl(_txtSupplierName);
                    return true;
                }
                return false;
            }

            if (keyData == Keys.Right)
            {
                if (editTb != null && !CanLeaveTextBoxRight(editTb))
                {
                    return false; // Let caret move right inside the text
                }

                _grid.EndEdit();
                for (int c = colIdx + 1; c < _grid.Columns.Count; c++)
                {
                    if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                    {
                        _grid.CurrentCell = _grid.Rows[rowIdx].Cells[c];
                        _grid.BeginEdit(true);
                        return true;
                    }
                }

                // At last editable column: move to next row's ProductName
                if (rowIdx + 1 < _grid.Rows.Count && _grid.Columns.Contains("ProductName"))
                {
                    _grid.CurrentCell = _grid.Rows[rowIdx + 1].Cells["ProductName"];
                    _grid.BeginEdit(true);
                    return true;
                }
                return false;
            }

            if (keyData == Keys.Up)
            {
                _grid.EndEdit();
                if (rowIdx > 0)
                {
                    _grid.CurrentCell = _grid.Rows[rowIdx - 1].Cells[colIdx];
                    _grid.BeginEdit(true);
                    return true;
                }

                // From Row 0, Up jumps naturally into the header field right above the current column
                string colName = _grid.Columns[colIdx].Name;
                if (colName == "ProductName" || colName == "Qty")
                {
                    FocusHeaderControl(_txtSupplierName);
                }
                else if (colName == "Hsn" || colName == "TaxPct" || colName == "PPrice" || colName == "SPrice" || colName == "Mrp")
                {
                    FocusHeaderControl(_txtCoinAdj);
                }
                else
                {
                    FocusHeaderControl(_txtAmountPayable);
                }
                return true;
            }

            if (keyData == Keys.Down)
            {
                _grid.EndEdit();
                if (rowIdx + 1 < _grid.Rows.Count)
                {
                    _grid.CurrentCell = _grid.Rows[rowIdx + 1].Cells[colIdx];
                    _grid.BeginEdit(true);
                    return true;
                }
                return true;
            }

            return false;
        }

        private void DeleteCurrentGridRow()
        {
            if (_grid.CurrentCell == null) return;
            int rowIdx = _grid.CurrentCell.RowIndex;

            if (_productDropdown != null) _productDropdown.HidePopup();
            _suppressSuggest = true;
            _grid.CancelEdit();
            _grid.EndEdit();

            if (!_grid.Rows[rowIdx].IsNewRow && _grid.Rows.Count > 1)
            {
                _grid.Rows.RemoveAt(rowIdx);
            }
            else
            {
                foreach (DataGridViewCell cell in _grid.Rows[rowIdx].Cells)
                {
                    if (!cell.ReadOnly || _grid.Columns[cell.ColumnIndex].Name == "PCode")
                    {
                        cell.Value = null;
                    }
                }
            }

            if (_grid.Rows.Count == 0)
            {
                _grid.Rows.Add();
            }

            RenumberRows();
            RecalculateAll();
            UpdateSelectedRowMarginPanel();
            _suppressSuggest = false;

            int targetRow = Math.Min(rowIdx, _grid.Rows.Count - 1);
            if (targetRow >= 0 && _grid.Columns.Contains("ProductName"))
            {
                _grid.CurrentCell = _grid.Rows[targetRow].Cells["ProductName"];
                _grid.BeginEdit(true);
            }
        }

        private void MoveToNextGridFieldInRow()
        {
            if (_grid.CurrentCell == null) return;
            int rowIdx = _grid.CurrentCell.RowIndex;
            int colIdx = _grid.CurrentCell.ColumnIndex;
            string colName = _grid.Columns[colIdx].Name;

            _grid.EndEdit();

            if (_productDropdown != null && _productDropdown.Visible)
            {
                return;
            }

            if (colName == "SPrice" || colName == "Mrp")
            {
                decimal pprice = ParseDec(_grid.Rows[rowIdx].Cells["PPrice"].Value);
                decimal sprice = ParseDec(_grid.Rows[rowIdx].Cells["SPrice"].Value);
                if (sprice <= pprice)
                {
                    MessageBox.Show(
                        string.Format("SPrice ({0:0.00}) is smaller than PPrice ({1:0.00})!\n\nSPrice must always be greater than PPrice.", sprice, pprice),
                        "Price Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _grid.CurrentCell = _grid.Rows[rowIdx].Cells["SPrice"];
                    _grid.BeginEdit(true);
                    return;
                }
            }

            for (int c = colIdx + 1; c < _grid.Columns.Count; c++)
            {
                if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                {
                    _grid.CurrentCell = _grid.Rows[rowIdx].Cells[c];
                    _grid.BeginEdit(true);
                    return;
                }
            }

            int nextRow = rowIdx + 1;
            if (nextRow >= _grid.Rows.Count)
            {
                _grid.Rows.Add();
                nextRow = _grid.Rows.Count - 1;
            }
            if (nextRow < _grid.Rows.Count && _grid.Columns.Contains("ProductName"))
            {
                _grid.CurrentCell = _grid.Rows[nextRow].Cells["ProductName"];
                _grid.BeginEdit(true);
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (_grid.CurrentCell == null) return;
            int colIdx = _grid.CurrentCell.ColumnIndex;
            string colName = _grid.Columns[colIdx].Name;

            if (colName == "ProductName" && e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                _productDropdown.HidePopup();
                PickProductForRow(_grid.CurrentCell.RowIndex, Convert.ToString(_grid.CurrentCell.Value));
            }
            else if (colName == "BatchDesc" && e.KeyCode == Keys.F4)
            {
                e.Handled = true;
                PickBatchForRow(_grid.CurrentCell.RowIndex);
            }
        }

        private void Grid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            string colName = _grid.Columns[e.ColumnIndex].Name;

            if (colName == "ProductName" && !_suppressSuggest)
            {
                if (!_productDropdown.Visible)
                {
                    string typed = Convert.ToString(row.Cells["ProductName"].Value ?? string.Empty).Trim();
                    int existingCode = 0;
                    int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out existingCode);
                    if (!string.IsNullOrEmpty(typed) && existingCode == 0)
                    {
                        var matches = _repo.SearchProducts(typed);
                        if (matches.Count == 1)
                        {
                            PopulateRowFromProduct(row, matches[0]);
                        }
                        else if (matches.Count > 1)
                        {
                            _activeEditRowIndex = e.RowIndex;
                            ShowProductDropdownBelowCell(e.RowIndex, typed);
                            return;
                        }
                    }
                }
            }
            else if (colName == "SPrice")
            {
                // Auto-sync MRP = SPrice when entering Sale Price
                decimal sprice = ParseDec(row.Cells["SPrice"].Value);
                if (sprice > 0)
                {
                    string formatted = sprice.ToString("0.00", CultureInfo.InvariantCulture);
                    row.Cells["SPrice"].Value = formatted;
                    row.Cells["Mrp"].Value = formatted;
                }
            }
            else if (colName == "Mrp")
            {
                // Auto-sync SPrice = MRP if SPrice is 0
                decimal mrp = ParseDec(row.Cells["Mrp"].Value);
                decimal sprice = ParseDec(row.Cells["SPrice"].Value);
                if (sprice <= 0 && mrp > 0)
                {
                    row.Cells["SPrice"].Value = mrp.ToString("0.00", CultureInfo.InvariantCulture);
                }
            }

            RecalculateAll();
            UpdateSelectedRowMarginPanel();
        }

        private void PickProductForRow(int rowIndex, string filter)
        {
            using (var dlg = new ProductLookupForm(_repo, filter))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedProduct != null)
                {
                    PopulateRowFromProduct(_grid.Rows[rowIndex], dlg.SelectedProduct);
                    RecalculateAll();
                    UpdateSelectedRowMarginPanel();
                }
            }
        }

        private void OpenNewProductDialog(string initialName)
        {
            using (var dlg = new NewProductForm(_repo, null, initialName))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SavedProduct != null)
                {
                    int targetRow = _grid.CurrentRow != null ? _grid.CurrentRow.Index : _grid.Rows.Add();
                    PopulateRowFromProduct(_grid.Rows[targetRow], dlg.SavedProduct);
                    RecalculateAll();
                }
            }
        }

        private void PopulateRowFromProduct(DataGridViewRow row, Product p)
        {
            row.Cells["PCode"].Value = p.ProductCode;
            row.Cells["ProductName"].Value = p.ProductName;
            if (ParseDec(row.Cells["Qty"].Value) <= 0) row.Cells["Qty"].Value = "1";
            if (row.Cells["FreeQty"].Value == null) row.Cells["FreeQty"].Value = "0";
            if (row.Cells["DiscAmt"].Value == null) row.Cells["DiscAmt"].Value = "0.00";
            if (row.Cells["ProdDisPct"].Value == null) row.Cells["ProdDisPct"].Value = "0";
            row.Cells["Hsn"].Value = p.HsnCode;
            row.Cells["TaxPct"].Value = p.TaxPercent.ToString("0.##", CultureInfo.InvariantCulture);
            row.Cells["PPrice"].Value = p.PurchasePrice.ToString("0.00", CultureInfo.InvariantCulture);
            row.Cells["SPrice"].Value = p.SalePrice.ToString("0.00", CultureInfo.InvariantCulture);
            row.Cells["Mrp"].Value = p.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void PickBatchForRow(int rowIndex)
        {
            var row = _grid.Rows[rowIndex];
            int pcode;
            if (!int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode)) return;
            var batches = _repo.GetBatchesForProduct(pcode);
            if (batches.Count == 0) return;

            using (var dlg = new BatchLookupForm(batches, Convert.ToString(row.Cells["ProductName"].Value)))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedBatch != null)
                {
                    row.Cells["BatchDesc"].Value = dlg.SelectedBatch.BatchDesc;
                    row.Cells["Expiry"].Value = dlg.SelectedBatch.ExpiryMMYY;
                    row.Cells["PPrice"].Value = dlg.SelectedBatch.PPrice.ToString("0.00", CultureInfo.InvariantCulture);
                    row.Cells["SPrice"].Value = dlg.SelectedBatch.SPrice.ToString("0.00", CultureInfo.InvariantCulture);
                    row.Cells["Mrp"].Value = dlg.SelectedBatch.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
                    RecalculateAll();
                }
            }
        }

        private void RenumberRows()
        {
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                if (!_grid.Rows[i].IsNewRow)
                    _grid.Rows[i].Cells["Sno"].Value = i + 1;
            }
        }

        private decimal CalculateGross()
        {
            decimal gross = 0m;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                decimal qty = ParseDec(row.Cells["Qty"].Value);
                decimal pprice = ParseDec(row.Cells["PPrice"].Value);
                decimal discAmt = ParseDec(row.Cells["DiscAmt"].Value);
                decimal prodDisPct = ParseDec(row.Cells["ProdDisPct"].Value);
                decimal lineGross = (qty * pprice) - discAmt;
                lineGross = lineGross * (1m - prodDisPct / 100m);
                gross += lineGross;
            }
            return gross;
        }

        private void RecalculateAll()
        {
            decimal headerDisPct = ParseDec(_txtDiscountPct.Text);
            decimal coinAdj = ParseDec(_txtCoinAdj.Text);
            decimal amountPayable = ParseDec(_txtAmountPayable.Text);

            var slabGross = new Dictionary<decimal, decimal>();
            decimal totalGross = 0m;
            decimal totalMrp = 0m;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                decimal qty = ParseDec(row.Cells["Qty"].Value);
                decimal freeQty = ParseDec(row.Cells["FreeQty"].Value);
                decimal pprice = ParseDec(row.Cells["PPrice"].Value);
                decimal mrp = ParseDec(row.Cells["Mrp"].Value);
                decimal discAmt = ParseDec(row.Cells["DiscAmt"].Value);
                decimal prodDisPct = ParseDec(row.Cells["ProdDisPct"].Value);
                decimal taxPct = ParseDec(row.Cells["TaxPct"].Value);

                decimal lineGross = ((qty * pprice) - discAmt) * (1m - prodDisPct / 100m);
                decimal taxable = lineGross * (1m - headerDisPct / 100m);

                totalGross += taxable;
                totalMrp += (qty + freeQty) * mrp;

                if (!slabGross.ContainsKey(taxPct)) slabGross[taxPct] = 0m;
                slabGross[taxPct] += taxable;
            }

            _taxGrid.Rows.Clear();
            decimal totalSgst = 0m, totalCgst = 0m;
            foreach (var kv in slabGross.OrderBy(k => k.Key))
            {
                decimal sgst = Math.Round(kv.Value * (kv.Key / 200m), 2);
                decimal cgst = Math.Round(kv.Value * (kv.Key / 200m), 2);
                decimal net = kv.Value + sgst + cgst;
                totalSgst += sgst;
                totalCgst += cgst;
                _taxGrid.Rows.Add(
                    kv.Key.ToString("0.##") + "%",
                    kv.Value.ToString("0.00"),
                    sgst.ToString("0.00"),
                    cgst.ToString("0.00"),
                    net.ToString("0.00"));
            }

            decimal calcNet = Math.Round(totalGross + totalSgst + totalCgst + coinAdj, 2);
            decimal diff = Math.Round(amountPayable - calcNet, 2);

            _lblTotalTaxSummary.Text = string.Format("Gross: {0:0.00}   SGST: {1:0.00}   CGST: {2:0.00}", totalGross, totalSgst, totalCgst);
            _lblMrpTotal.Text = string.Format("Mrp Total : {0:0.00}", totalMrp);
            _lblNetPayableSummary.Text = string.Format("Calculated Net : {0:0.00}", calcNet);
            _lblDiff.Text = string.Format("Diff : {0:0.00}", diff);
            _lblDiff.ForeColor = diff == 0m ? Color.DarkGreen : Color.DarkRed;
        }

        private void UpdateSelectedRowMarginPanel()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            var row = _grid.CurrentRow;
            int pcode;
            if (int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode) && pcode > 0)
            {
                var prod = _repo.GetProductByCode(pcode);
                if (prod != null)
                {
                    _lblTotalStock.Text = string.Format("Total Stock : {0:0.##}", prod.TotalStockUnits);
                }
            }

            decimal pprice = ParseDec(row.Cells["PPrice"].Value);
            decimal mrp = ParseDec(row.Cells["Mrp"].Value);
            decimal tax = ParseDec(row.Cells["TaxPct"].Value);
            decimal cost = pprice * (1m + tax / 100m);
            decimal marginPct = cost > 0 ? ((mrp - cost) / cost) * 100m : 0m;

            _lblItemCost.Text = string.Format("Item Cost : {0:0.00}", cost);
            _lblCurMargin.Text = string.Format("Cur.Margin : {0:0.00} %", marginPct);
        }

        private void ProcessAndSaveGrn()
        {
            _grid.EndEdit();
            RecalculateAll();

            if (_selectedSupplier == null)
            {
                MessageBox.Show("Please select a Supplier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtSupplierName.Focus();
                return;
            }
            if (_txtInvoiceNo.Text == null || _txtInvoiceNo.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter Supplier Invoice No.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtInvoiceNo.Focus();
                return;
            }

            var lines = new List<PurchaseLine>();
            int sno = 1;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                int pcode;
                if (!int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode) || pcode <= 0) continue;

                string batch = Convert.ToString(row.Cells["BatchDesc"].Value ?? string.Empty).Trim().ToUpperInvariant();
                string exp = Convert.ToString(row.Cells["Expiry"].Value ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(batch) || string.IsNullOrEmpty(exp))
                {
                    MessageBox.Show("Batch Desc and Expiry (MM/YY) are required for row " + sno, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal pprice = ParseDec(row.Cells["PPrice"].Value);
                decimal mrp = ParseDec(row.Cells["Mrp"].Value);
                decimal sprice = ParseDec(row.Cells["SPrice"].Value);
                if (sprice <= 0) sprice = mrp;
                if (sprice <= pprice)
                {
                    MessageBox.Show(
                        string.Format("Row {0}: SPrice ({1:0.00}) is smaller than PPrice ({2:0.00})!\n\nSPrice must always be greater than PPrice.", sno, sprice, pprice),
                        "Price Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _grid.CurrentCell = row.Cells["SPrice"];
                    _grid.BeginEdit(true);
                    return;
                }
                decimal tax = ParseDec(row.Cells["TaxPct"].Value);
                decimal cost = pprice * (1m + tax / 100m);

                lines.Add(new PurchaseLine
                {
                    Sno = sno++,
                    ProductCode = pcode,
                    ProductName = Convert.ToString(row.Cells["ProductName"].Value),
                    Packing = string.Empty,
                    QtyPacks = ParseDec(row.Cells["Qty"].Value),
                    FreeQtyPacks = ParseDec(row.Cells["FreeQty"].Value),
                    DiscountAmt = ParseDec(row.Cells["DiscAmt"].Value),
                    ProdDisPercent = ParseDec(row.Cells["ProdDisPct"].Value),
                    BatchDesc = batch,
                    ExpiryMMYY = exp,
                    HsnCode = Convert.ToString(row.Cells["Hsn"].Value),
                    TaxPercent = tax,
                    PPrice = pprice,
                    SPrice = sprice,
                    Mrp = mrp,
                    ItemCost = Math.Round(cost, 2),
                    MarginPercent = cost > 0 ? Math.Round(((mrp - cost) / cost) * 100m, 2) : 0m
                });
            }

            if (lines.Count == 0)
            {
                MessageBox.Show("Please add at least one product line.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal gross = 0m, sgst = 0m, cgst = 0m;
            decimal headerDisPct = ParseDec(_txtDiscountPct.Text);
            var slabGross = new Dictionary<decimal, decimal>();
            foreach (var l in lines)
            {
                decimal lineGross = ((l.QtyPacks * l.PPrice) - l.DiscountAmt) * (1m - l.ProdDisPercent / 100m);
                decimal taxable = lineGross * (1m - headerDisPct / 100m);
                gross += taxable;
                if (!slabGross.ContainsKey(l.TaxPercent)) slabGross[l.TaxPercent] = 0m;
                slabGross[l.TaxPercent] += taxable;
            }
            foreach (var kv in slabGross)
            {
                sgst += Math.Round(kv.Value * (kv.Key / 200m), 2);
                cgst += Math.Round(kv.Value * (kv.Key / 200m), 2);
            }
            decimal coinAdj = ParseDec(_txtCoinAdj.Text);
            decimal net = Math.Round(gross + sgst + cgst + coinAdj, 2);
            decimal amountPayable = ParseDec(_txtAmountPayable.Text);
            decimal diff = Math.Round(amountPayable - net, 2);

            if (diff != 0m)
            {
                MessageBox.Show(
                    string.Format(
                        "Cannot process Purchase Entry! There is a Difference of {0:0.00}.\n\nAmount Payable : {1:0.00}\nCalculated Net : {2:0.00}\n\nPlease check the item amounts, Amount Payable, or Coin Adj. so Diff is 0.00 before processing.",
                        diff, amountPayable, net),
                    "Difference Amount Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtCoinAdj.Focus();
                _txtCoinAdj.SelectAll();
                return;
            }

            var header = new PurchaseHeader
            {
                GrnDate = _txtGrnDate.Text.Trim(),
                SupplierCode = _selectedSupplier.SupplierCode,
                SupplierName = _selectedSupplier.SupplierName,
                InvoiceNo = _txtInvoiceNo.Text.Trim(),
                InvoiceDate = _txtInvoiceDate.Text.Trim(),
                PaymentDate = _txtPaymentDate.Text.Trim(),
                Series = _cmbSeries.SelectedItem.ToString(),
                AmountPayable = amountPayable,
                DiscountPercent = headerDisPct,
                DiscountAmt = ParseDec(_txtDiscountAmt.Text),
                CoinAdj = coinAdj,
                GrossAmt = Math.Round(gross, 2),
                SgstAmt = sgst,
                CgstAmt = cgst,
                NetAmt = net,
                CreatedBy = _settings.DefaultUser
            };

            int grnNo = _repo.SavePurchaseGrn(header, lines);
            if (GrnSaved != null) GrnSaved();

            MessageBox.Show(
                string.Format("Purchase Updated Successfully! GRN Number :{0}", grnNo),
                "Shopaid.Net-RE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetForm();
        }

        private static decimal ParseDec(object val)
        {
            if (val == null) return 0m;
            decimal d;
            decimal.TryParse(Convert.ToString(val).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
            return d;
        }
    }
}
