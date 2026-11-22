using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;
using PharmaAidPro.WinForms.Printing;

namespace PharmaAidPro.WinForms.Forms
{
    public class SalesPosForm : Form
    {
        private class ParkedBill
        {
            public string CustomerName;
            public string DoctorName;
            public List<SalesLine> Lines;
            public string ParkedTime;
        }

        private static readonly List<ParkedBill> _parkedBills = new List<ParkedBill>();

        private readonly PharmaRepository _repo;
        private readonly StoreSettings _settings;

        private Label _lblBillNo;
        private Label _lblBillDate;
        private TextBox _txtCustomer;
        private TextBox _txtDoctor;
        private ComboBox _cmbPayMode;
        private Label _lblBigTotal;
        private DataGridView _grid;

        private Label _lblBaseAmt;
        private Label _lblSgstAmt;
        private Label _lblCgstAmt;
        private Label _lblTotalGst;
        private Label _lblRod;
        private Label _lblItemStats;
        private Label _lblRowStockHint;
        private CheckBox _chkPreviewOnSave;

        private InlineSuggestPanel<Product> _productDropdown;
        private TextBox _activeProductTextBox;
        private int _activeEditRowIndex = -1;
        private bool _suppressSuggest;

        public event Action SaleCompleted;

        public SalesPosForm(PharmaRepository repo)
        {
            _repo = repo;
            _settings = _repo.GetStoreSettings();
            InitializeComponent();
            ResetBill();
        }

        private void InitializeComponent()
        {
            this.Text = "Retail Sales Bill (F2) - " + _settings.StoreName;
            this.Size = new Size(1180, 660);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(236, 240, 246);

            // Header
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Color.FromArgb(230, 236, 246), Padding = new Padding(10) };

            headerPanel.Controls.Add(new Label { Text = "Bill No :", Left = 12, Top = 12, Width = 58, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _lblBillNo = new Label { Left = 74, Top = 11, Width = 120, Font = new Font("Tahoma", 10F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            headerPanel.Controls.Add(_lblBillNo);

            headerPanel.Controls.Add(new Label { Text = "Date :", Left = 205, Top = 12, Width = 45, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _lblBillDate = new Label { Left = 252, Top = 12, Width = 160 };
            headerPanel.Controls.Add(_lblBillDate);

            headerPanel.Controls.Add(new Label { Text = "Mode :", Left = 420, Top = 12, Width = 50, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _cmbPayMode = new ComboBox { Left = 472, Top = 8, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbPayMode.Items.AddRange(new object[] { "CASH", "UPI", "CARD", "CREDIT" });
            _cmbPayMode.SelectedIndex = 0;
            headerPanel.Controls.Add(_cmbPayMode);

            headerPanel.Controls.Add(new Label { Text = "Customer :", Left = 12, Top = 44, Width = 72 });
            _txtCustomer = new TextBox { Left = 88, Top = 41, Width = 240, CharacterCasing = CharacterCasing.Upper };
            headerPanel.Controls.Add(_txtCustomer);

            headerPanel.Controls.Add(new Label { Text = "Doctor :", Left = 345, Top = 44, Width = 55 });
            _txtDoctor = new TextBox { Left = 405, Top = 41, Width = 210, CharacterCasing = CharacterCasing.Upper };
            headerPanel.Controls.Add(_txtDoctor);

            // Big Total Banner (top right)
            var totalBox = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(255, 250, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lblBigTotal = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Tahoma", 20F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Padding = new Padding(0, 0, 12, 0),
                Text = "Rs 0.00"
            };
            totalBox.Controls.Add(_lblBigTotal);
            headerPanel.Controls.Add(totalBox);

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

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sno", HeaderText = "S.No", ReadOnly = true, FillWeight = 5 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PCode", HeaderText = "P.Code", ReadOnly = true, FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Particulars (Product Name)", FillWeight = 27 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty (Units)", FillWeight = 8, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BatchDesc", HeaderText = "Batch No (F4)", FillWeight = 11 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expiry", HeaderText = "Exp", FillWeight = 7 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate (MRP)", FillWeight = 9, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SaleUnit", HeaderText = "Pack", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisPct", HeaderText = "Dis%", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TaxPct", HeaderText = "GST%", FillWeight = 6, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", ReadOnly = true, FillWeight = 10, DefaultCellStyle = RightStyle() });

            _grid.RowsAdded += delegate { RenumberRows(); };
            _grid.EditingControlShowing += Grid_EditingControlShowing;
            _grid.CellEndEdit += Grid_CellEndEdit;
            _grid.SelectionChanged += delegate { UpdateRowStockHint(); };
            _grid.KeyDown += Grid_KeyDown;

            _productDropdown = new InlineSuggestPanel<Product>(
                "Up/Down: Navigate  |  Enter: Select Product  |  F2: Full Search  |  Esc: Close",
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "P_code", FillWeight = 13 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", FillWeight = 44 },
                new DataGridViewTextBoxColumn { DataPropertyName = "TotalStockUnits", HeaderText = "Stock", FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { DataPropertyName = "SaleUnit", HeaderText = "Pack", FillWeight = 11, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { DataPropertyName = "Mrp", HeaderText = "MRP", FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _productDropdown.ItemChosen += ProductDropdown_ItemChosen;

            // Bottom Panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 105, BackColor = Color.FromArgb(232, 237, 245), Padding = new Padding(8) };

            _lblRowStockHint = new Label { Left = 12, Top = 8, Width = 540, Height = 20, ForeColor = Color.DarkBlue, Font = new Font("Tahoma", 8.5F, FontStyle.Bold) };
            _lblBaseAmt = new Label { Left = 12, Top = 32, Width = 140, Text = "Base : 0.00" };
            _lblSgstAmt = new Label { Left = 160, Top = 32, Width = 120, Text = "SGST : 0.00" };
            _lblCgstAmt = new Label { Left = 290, Top = 32, Width = 120, Text = "CGST : 0.00" };
            _lblTotalGst = new Label { Left = 420, Top = 32, Width = 130, Text = "GST : 0.00" };
            _lblRod = new Label { Left = 560, Top = 32, Width = 110, Text = "ROD : 0.00" };
            _lblItemStats = new Label { Left = 680, Top = 32, Width = 240, Font = new Font("Tahoma", 8.5F, FontStyle.Bold), Text = "Items : 0   Total Qty : 0" };

            _chkPreviewOnSave = new CheckBox
            {
                Text = "Show Receipt Preview",
                Left = 930,
                Top = 8,
                Width = 175,
                Checked = (_settings.PrinterName == null || _settings.PrinterName.Trim().Length == 0)
            };

            bottomPanel.Controls.AddRange(new Control[]
            {
                _lblRowStockHint, _lblBaseAmt, _lblSgstAmt, _lblCgstAmt, _lblTotalGst, _lblRod, _lblItemStats, _chkPreviewOnSave
            });

            // Function Buttons Strip
            int bx = 12;
            AddFnButton(bottomPanel, "F1: Save & Print", ref bx, 130, Color.FromArgb(210, 228, 250), delegate { SaveAndPrintBill(); });
            AddFnButton(bottomPanel, "F3: Generic Sub", ref bx, 120, Color.White, delegate { ShowGenericSubstitute(); });
            AddFnButton(bottomPanel, "F4: Batch List", ref bx, 110, Color.White, delegate { PickBatchForCurrentRow(); });
            AddFnButton(bottomPanel, "F5: Hold Bill", ref bx, 105, Color.White, delegate { HoldCurrentBill(); });
            AddFnButton(bottomPanel, "F6: Recall (" + _parkedBills.Count + ")", ref bx, 115, Color.White, delegate { RecallHeldBill(); });
            AddFnButton(bottomPanel, "F8: Bill Copy", ref bx, 105, Color.White, delegate { using (var f = new BillCopyForm(_repo)) f.ShowDialog(this); });
            AddFnButton(bottomPanel, "Clear", ref bx, 80, Color.White, delegate { ResetBill(); });
            AddFnButton(bottomPanel, "Esc: Close", ref bx, 90, Color.White, delegate { this.Close(); });

            this.Controls.Add(_grid);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(_productDropdown);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.F1) { e.Handled = true; SaveAndPrintBill(); }
                else if (e.KeyCode == Keys.F3) { e.Handled = true; ShowGenericSubstitute(); }
                else if (e.KeyCode == Keys.F4) { e.Handled = true; PickBatchForCurrentRow(); }
                else if (e.KeyCode == Keys.F5) { e.Handled = true; HoldCurrentBill(); }
                else if (e.KeyCode == Keys.F6) { e.Handled = true; RecallHeldBill(); }
                else if (e.KeyCode == Keys.F8) { e.Handled = true; using (var f = new BillCopyForm(_repo)) f.ShowDialog(this); }
                else if (e.KeyCode == Keys.Escape) { this.Close(); }
            };
        }

        private static DataGridViewCellStyle RightStyle()
        {
            return new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
        }

        private static void AddFnButton(Panel parent, string text, ref int x, int w, Color bg, Action onClick)
        {
            var btn = new Button { Text = text, Left = x, Top = 62, Width = w, Height = 30, BackColor = bg };
            btn.Click += delegate { onClick(); };
            parent.Controls.Add(btn);
            x += w + 8;
        }

        private void ResetBill()
        {
            _lblBillNo.Text = _repo.GetNextBillNo();
            _lblBillDate.Text = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
            _txtCustomer.Text = string.Empty;
            _txtDoctor.Text = string.Empty;
            _cmbPayMode.SelectedIndex = 0;
            _grid.Rows.Clear();
            RecalculateTotals();
            _lblRowStockHint.Text = "Type product name in grid and press Enter to search.";
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

            var matches = _repo.SearchProducts(query);
            _productDropdown.ShowAt(this, hostPt, matches);
        }

        private void ProductDropdown_ItemChosen(Product prod)
        {
            int targetRow = _activeEditRowIndex >= 0 && _activeEditRowIndex < _grid.Rows.Count
                ? _activeEditRowIndex
                : (_grid.CurrentRow != null ? _grid.CurrentRow.Index : -1);
            if (targetRow < 0) return;

            _suppressSuggest = true;
            _grid.EndEdit();
            AssignProductToRow(_grid.Rows[targetRow], prod);
            _suppressSuggest = false;

            if (targetRow < _grid.Rows.Count)
            {
                _grid.CurrentCell = _grid.Rows[targetRow].Cells["Qty"];
                _grid.BeginEdit(true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_productDropdown != null && _productDropdown.Visible)
            {
                if (keyData == Keys.Down)
                {
                    _productDropdown.MoveSelection(1);
                    return true;
                }
                if (keyData == Keys.Up)
                {
                    _productDropdown.MoveSelection(-1);
                    return true;
                }
                if (keyData == Keys.Enter)
                {
                    if (_productDropdown.ConfirmCurrent())
                    {
                        return true;
                    }
                }
                if (keyData == Keys.Escape)
                {
                    _productDropdown.HidePopup();
                    return true;
                }
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
                if (keyData == Keys.Back)
                {
                    if (ShouldMoveBackwardOnBackspace())
                    {
                        MoveToPreviousGridFieldInRow();
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool ShouldMoveBackwardOnBackspace()
        {
            if (_grid.CurrentCell == null) return false;
            string colName = _grid.Columns[_grid.CurrentCell.ColumnIndex].Name;
            if (colName == "ProductName" && _grid.IsCurrentCellInEditMode)
            {
                var tb = _grid.EditingControl as TextBox;
                if (tb != null && tb.Text.Length > 0 && tb.SelectionLength < tb.Text.Length && tb.SelectionStart > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void MoveToPreviousGridFieldInRow()
        {
            if (_grid.CurrentCell == null) return;
            int rowIdx = _grid.CurrentCell.RowIndex;
            int colIdx = _grid.CurrentCell.ColumnIndex;

            if (_productDropdown != null) _productDropdown.HidePopup();
            _grid.EndEdit();

            for (int c = colIdx - 1; c >= 0; c--)
            {
                if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                {
                    _grid.CurrentCell = _grid.Rows[rowIdx].Cells[c];
                    _grid.BeginEdit(true);
                    return;
                }
            }

            if (rowIdx > 0)
            {
                int prevRow = rowIdx - 1;
                for (int c = _grid.Columns.Count - 1; c >= 0; c--)
                {
                    if (_grid.Columns[c].Visible && !_grid.Columns[c].ReadOnly)
                    {
                        _grid.CurrentCell = _grid.Rows[prevRow].Cells[c];
                        _grid.BeginEdit(true);
                        return;
                    }
                }
            }
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
                    if (!cell.ReadOnly || _grid.Columns[cell.ColumnIndex].Name == "PCode" || _grid.Columns[cell.ColumnIndex].Name == "Amount")
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
            RecalculateTotals();
            UpdateRowStockHint();
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

            _grid.EndEdit();

            // If CellEndEdit opened the product suggestion dropdown, let the user pick from it first
            if (_productDropdown != null && _productDropdown.Visible)
            {
                return;
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

            // Reached the end of editable columns in this row -> move to ProductName in the next row
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
            string colName = _grid.Columns[_grid.CurrentCell.ColumnIndex].Name;
            if (colName == "ProductName" && e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                _productDropdown.HidePopup();
                PickProductForRow(_grid.CurrentCell.RowIndex, Convert.ToString(_grid.CurrentCell.Value));
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
                    int pcode = 0;
                    int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode);
                    if (!string.IsNullOrEmpty(typed) && pcode == 0)
                    {
                        var matches = _repo.SearchProducts(typed);
                        if (matches.Count == 1)
                        {
                            AssignProductToRow(row, matches[0]);
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
            else if (_grid.Columns[e.ColumnIndex].Name == "Expiry")
            {
                string expRaw = Convert.ToString(row.Cells["Expiry"].Value ?? string.Empty).Trim().Replace("-", "/");
                if (expRaw.Length == 4 && !expRaw.Contains("/"))
                {
                    expRaw = expRaw.Substring(0, 2) + "/" + expRaw.Substring(2, 2);
                }
                row.Cells["Expiry"].Value = expRaw;
            }

            RecalculateRow(row);
            RecalculateTotals();
            UpdateRowStockHint();
        }

        private void PickProductForRow(int rowIndex, string filter)
        {
            using (var dlg = new ProductLookupForm(_repo, filter))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedProduct != null)
                {
                    AssignProductToRow(_grid.Rows[rowIndex], dlg.SelectedProduct);
                }
            }
        }

        private void AssignProductToRow(DataGridViewRow row, Product prod)
        {
            var batches = _repo.GetBatchesForProduct(prod.ProductCode);
            var fifoBatch = batches.FirstOrDefault(b => b.StockBaseUnits > 0) ?? batches.FirstOrDefault();

            row.Cells["PCode"].Value = prod.ProductCode;
            row.Cells["ProductName"].Value = prod.ProductName;
            row.Cells["SaleUnit"].Value = prod.SaleUnit > 0 ? prod.SaleUnit : 1;
            row.Cells["TaxPct"].Value = prod.TaxPercent.ToString("0.##", CultureInfo.InvariantCulture);
            if (ParseDec(row.Cells["Qty"].Value) <= 0) row.Cells["Qty"].Value = prod.SaleUnit > 0 ? prod.SaleUnit.ToString() : "1";
            if (row.Cells["DisPct"].Value == null) row.Cells["DisPct"].Value = "0";

            if (fifoBatch != null)
            {
                row.Cells["BatchDesc"].Value = fifoBatch.BatchDesc;
                row.Cells["Expiry"].Value = fifoBatch.ExpiryMMYY;
                row.Cells["Rate"].Value = fifoBatch.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
            }
            else
            {
                row.Cells["BatchDesc"].Value = "DEFAULT";
                row.Cells["Expiry"].Value = "12/28";
                row.Cells["Rate"].Value = prod.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
            }

            RecalculateRow(row);
            RecalculateTotals();
            UpdateRowStockHint();
        }

        private void PickBatchForCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            var row = _grid.CurrentRow;
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
                    row.Cells["Rate"].Value = dlg.SelectedBatch.Mrp.ToString("0.00", CultureInfo.InvariantCulture);
                    RecalculateRow(row);
                    RecalculateTotals();
                    UpdateRowStockHint();
                }
            }
        }

        private void ShowGenericSubstitute()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            int pcode;
            if (!int.TryParse(Convert.ToString(_grid.CurrentRow.Cells["PCode"].Value), out pcode)) return;

            var subs = _repo.FindGenericSubstitutes(pcode);
            if (subs.Count == 0)
            {
                MessageBox.Show("No alternate generic brands found in stock for this composition.", "Generic Substitute (F3)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new Form { Text = "Generic Substitutes (F3)", Size = new Size(640, 280), StartPosition = FormStartPosition.CenterParent })
            {
                var g = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    DataSource = subs
                };
                g.CellDoubleClick += delegate
                {
                    if (g.CurrentRow != null && g.CurrentRow.DataBoundItem is Product)
                    {
                        AssignProductToRow(_grid.CurrentRow, (Product)g.CurrentRow.DataBoundItem);
                        form.Close();
                    }
                };
                form.Controls.Add(g);
                form.ShowDialog(this);
            }
        }

        private void HoldCurrentBill()
        {
            var lines = CollectLines();
            if (lines.Count == 0) return;

            _parkedBills.Add(new ParkedBill
            {
                CustomerName = _txtCustomer.Text.Trim(),
                DoctorName = _txtDoctor.Text.Trim(),
                Lines = lines,
                ParkedTime = DateTime.Now.ToString("hh:mm tt")
            });
            ResetBill();
            MessageBox.Show("Bill parked! Press F6 to recall it.", "Hold Bill (F5)", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RecallHeldBill()
        {
            if (_parkedBills.Count == 0)
            {
                MessageBox.Show("No parked bills.", "Recall Bill (F6)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var parked = _parkedBills[_parkedBills.Count - 1];
            _parkedBills.RemoveAt(_parkedBills.Count - 1);

            ResetBill();
            _txtCustomer.Text = parked.CustomerName;
            _txtDoctor.Text = parked.DoctorName;
            foreach (var l in parked.Lines)
            {
                int idx = _grid.Rows.Add();
                var r = _grid.Rows[idx];
                r.Cells["PCode"].Value = l.ProductCode;
                r.Cells["ProductName"].Value = l.ProductName;
                r.Cells["Qty"].Value = l.QtyUnits;
                r.Cells["BatchDesc"].Value = l.BatchDesc;
                r.Cells["Expiry"].Value = l.ExpiryMMYY;
                r.Cells["Rate"].Value = l.RateMrp.ToString("0.00", CultureInfo.InvariantCulture);
                r.Cells["SaleUnit"].Value = l.SaleUnit;
                r.Cells["DisPct"].Value = l.DiscPercent;
                r.Cells["TaxPct"].Value = l.TaxPercent;
                RecalculateRow(r);
            }
            RecalculateTotals();
        }

        private void RenumberRows()
        {
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                if (!_grid.Rows[i].IsNewRow)
                    _grid.Rows[i].Cells["Sno"].Value = i + 1;
            }
        }

        private static void RecalculateRow(DataGridViewRow row)
        {
            if (row.IsNewRow) return;
            decimal qty = ParseDec(row.Cells["Qty"].Value);
            decimal rate = ParseDec(row.Cells["Rate"].Value);
            decimal saleUnit = ParseDec(row.Cells["SaleUnit"].Value);
            if (saleUnit <= 0) saleUnit = 1m;
            decimal disPct = ParseDec(row.Cells["DisPct"].Value);

            decimal amount = Math.Round((qty / saleUnit) * rate * (1m - disPct / 100m), 2);
            row.Cells["Amount"].Value = amount.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void RecalculateTotals()
        {
            decimal total = 0m, baseSum = 0m, gstSum = 0m, qtySum = 0m;
            int itemCount = 0;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                int pcode;
                if (!int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode) || pcode <= 0) continue;

                decimal amt = ParseDec(row.Cells["Amount"].Value);
                decimal taxPct = ParseDec(row.Cells["TaxPct"].Value);
                decimal lineBase = taxPct > 0 ? amt / (1m + taxPct / 100m) : amt;

                total += amt;
                baseSum += lineBase;
                gstSum += (amt - lineBase);
                qtySum += ParseDec(row.Cells["Qty"].Value);
                itemCount++;
            }

            decimal sgst = Math.Round(gstSum / 2m, 2);
            decimal cgst = Math.Round(gstSum / 2m, 2);
            decimal net = Math.Round(total, 2);
            decimal rod = 0m;

            _lblBaseAmt.Text = string.Format("Base : {0:0.00}", baseSum);
            _lblSgstAmt.Text = string.Format("SGST : {0:0.00}", sgst);
            _lblCgstAmt.Text = string.Format("CGST : {0:0.00}", cgst);
            _lblTotalGst.Text = string.Format("GST : {0:0.00}", gstSum);
            _lblRod.Text = string.Format("ROD : {0:0.##}", rod);
            _lblItemStats.Text = string.Format("Items : {0}   Total Qty : {1:0.##}", itemCount, qtySum);
            _lblBigTotal.Text = string.Format("Rs {0:0.00}", net);
        }

        private void UpdateRowStockHint()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            int pcode;
            if (!int.TryParse(Convert.ToString(_grid.CurrentRow.Cells["PCode"].Value), out pcode) || pcode <= 0) return;

            var prod = _repo.GetProductByCode(pcode);
            if (prod != null)
            {
                _lblRowStockHint.Text = string.Format(
                    "{0} | Rack: {1} | Pack: {2} | Total Stock: {3:0.##} units",
                    prod.ProductName, prod.Rack, prod.SaleUnit, prod.TotalStockUnits);
            }
        }

        private List<SalesLine> CollectLines()
        {
            var lines = new List<SalesLine>();
            int sno = 1;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                int pcode;
                if (!int.TryParse(Convert.ToString(row.Cells["PCode"].Value), out pcode) || pcode <= 0) continue;

                decimal qty = ParseDec(row.Cells["Qty"].Value);
                if (qty <= 0) continue;

                lines.Add(new SalesLine
                {
                    Sno = sno++,
                    ProductCode = pcode,
                    ProductName = Convert.ToString(row.Cells["ProductName"].Value),
                    QtyUnits = qty,
                    BatchDesc = Convert.ToString(row.Cells["BatchDesc"].Value),
                    ExpiryMMYY = Convert.ToString(row.Cells["Expiry"].Value),
                    RateMrp = ParseDec(row.Cells["Rate"].Value),
                    SaleUnit = ParseDec(row.Cells["SaleUnit"].Value),
                    DiscPercent = ParseDec(row.Cells["DisPct"].Value),
                    TaxPercent = ParseDec(row.Cells["TaxPct"].Value),
                    LineAmount = ParseDec(row.Cells["Amount"].Value)
                });
            }
            return lines;
        }

        private void SaveAndPrintBill()
        {
            var lines = CollectLines();
            if (lines.Count == 0)
            {
                MessageBox.Show("Please add at least one item to the bill.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = 0m, baseSum = 0m, gstSum = 0m, qtySum = 0m;
            foreach (var l in lines)
            {
                decimal lineBase = l.TaxPercent > 0 ? l.LineAmount / (1m + l.TaxPercent / 100m) : l.LineAmount;
                total += l.LineAmount;
                baseSum += lineBase;
                gstSum += (l.LineAmount - lineBase);
                qtySum += l.QtyUnits;
            }

            var header = new SalesHeader
            {
                BillDate = DateTime.Now.ToString("dd/MM/yy HH:mm:ss"),
                CustomerName = _txtCustomer.Text.Trim(),
                DoctorName = _txtDoctor.Text.Trim(),
                PaymentMode = _cmbPayMode.SelectedItem.ToString(),
                BaseAmt = Math.Round(baseSum, 2),
                SgstAmt = Math.Round(gstSum / 2m, 2),
                CgstAmt = Math.Round(gstSum / 2m, 2),
                TotalGstAmt = Math.Round(gstSum, 2),
                RoundOff = 0m,
                NetAmount = Math.Round(total, 2),
                TotalItems = lines.Count,
                TotalQty = qtySum,
                CreatedBy = _settings.DefaultUser
            };

            string billNo = _repo.SaveSaleBill(header, lines);
            header.BillNo = billNo;
            if (SaleCompleted != null) SaleCompleted();

            if (_chkPreviewOnSave.Checked)
            {
                DotMatrixReceiptPrinter.ShowPreviewDialog(this, _settings, header, lines);
            }
            else
            {
                string raw = DotMatrixReceiptPrinter.FormatEscPRawReceipt(_settings, header, lines);
                string err;
                if (!DotMatrixReceiptPrinter.SendRawTextToPrinter(_settings.PrinterName, raw, out err))
                {
                    MessageBox.Show("Saved Bill " + billNo + ", but printer returned: " + err, "Print Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            ResetBill();
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
