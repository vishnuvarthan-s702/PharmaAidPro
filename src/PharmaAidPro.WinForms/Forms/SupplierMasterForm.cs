using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class SupplierMasterForm : Form
    {
        private readonly PharmaRepository _repo;
        private TextBox _txtSearch;
        private DataGridView _grid;
        private TextBox _txtCode;
        private TextBox _txtName;
        private TextBox _txtAddress;
        private TextBox _txtPhone;
        private TextBox _txtGstin;
        private TextBox _txtState;
        private TextBox _txtOutstanding;
        private bool _isNewMode = true;

        public event Action DataChanged;

        public SupplierMasterForm(PharmaRepository repo)
        {
            _repo = repo;
            InitializeComponent();
            RefreshGrid();
            ResetForNew();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier / Distributor Master";
            this.Size = new Size(920, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.Font = new Font("Tahoma", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 244, 249);

            var topBar = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            topBar.Controls.Add(new Label { Text = "Search Supplier:", Left = 10, Top = 12, Width = 110, Font = new Font("Tahoma", 9F, FontStyle.Bold) });
            _txtSearch = new TextBox { Left = 124, Top = 9, Width = 280 };
            _txtSearch.TextChanged += delegate { RefreshGrid(); };
            var btnNew = new Button { Text = "New Supplier", Left = 420, Top = 7, Width = 110, Height = 26 };
            btnNew.Click += delegate { ResetForNew(); };
            topBar.Controls.AddRange(new Control[] { _txtSearch, btnNew });

            var editPanel = new Panel { Dock = DockStyle.Right, Width = 320, Padding = new Padding(10), BackColor = Color.FromArgb(232, 237, 245) };
            int y = 14;
            _txtCode = AddLabeledBox(editPanel, "Code:", ref y, true);
            _txtName = AddLabeledBox(editPanel, "Supplier Name:", ref y, false);
            _txtName.CharacterCasing = CharacterCasing.Upper;
            _txtAddress = AddLabeledBox(editPanel, "Address:", ref y, false);
            _txtPhone = AddLabeledBox(editPanel, "Phone:", ref y, false);
            _txtGstin = AddLabeledBox(editPanel, "GSTIN:", ref y, false);
            _txtGstin.CharacterCasing = CharacterCasing.Upper;
            _txtState = AddLabeledBox(editPanel, "State:", ref y, false);
            _txtState.CharacterCasing = CharacterCasing.Upper;
            _txtOutstanding = AddLabeledBox(editPanel, "Outstanding Rs:", ref y, false);

            var btnSave = new Button { Text = "Save Supplier", Left = 110, Top = y + 10, Width = 185, Height = 30, BackColor = Color.FromArgb(220, 232, 248) };
            btnSave.Click += BtnSave_Click;
            editPanel.Controls.Add(btnSave);

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
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierCode", HeaderText = "Code", FillWeight = 12 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier Name", FillWeight = 36 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Gstin", HeaderText = "GSTIN", FillWeight = 24 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", FillWeight = 16 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OutstandingAmount", HeaderText = "Outstanding", FillWeight = 16, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });

            _grid.SelectionChanged += delegate
            {
                if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is Supplier)
                {
                    LoadSupplier((Supplier)_grid.CurrentRow.DataBoundItem);
                }
            };

            this.Controls.Add(_grid);
            this.Controls.Add(editPanel);
            this.Controls.Add(topBar);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) this.Close();
            };
        }

        private static TextBox AddLabeledBox(Panel parent, string label, ref int y, bool readOnly)
        {
            parent.Controls.Add(new Label { Text = label, Left = 10, Top = y + 3, Width = 98 });
            var tb = new TextBox { Left = 110, Top = y, Width = 190, ReadOnly = readOnly };
            parent.Controls.Add(tb);
            y += 32;
            return tb;
        }

        private void RefreshGrid()
        {
            List<Supplier> list = _repo.SearchSuppliers(_txtSearch.Text);
            _grid.DataSource = list;
        }

        private void ResetForNew()
        {
            _isNewMode = true;
            _txtCode.Text = _repo.GetNextSupplierCode().ToString();
            _txtName.Text = string.Empty;
            _txtAddress.Text = string.Empty;
            _txtPhone.Text = string.Empty;
            _txtGstin.Text = string.Empty;
            _txtState.Text = "TAMILNADU";
            _txtOutstanding.Text = "0.00";
            _txtName.Focus();
        }

        private void LoadSupplier(Supplier s)
        {
            _isNewMode = false;
            _txtCode.Text = s.SupplierCode.ToString();
            _txtName.Text = s.SupplierName;
            _txtAddress.Text = s.Address ?? string.Empty;
            _txtPhone.Text = s.Phone ?? string.Empty;
            _txtGstin.Text = s.Gstin ?? string.Empty;
            _txtState.Text = s.StateName ?? "TAMILNADU";
            _txtOutstanding.Text = s.OutstandingAmount.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_txtName.Text == null || _txtName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Supplier Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            decimal outAmt;
            decimal.TryParse(_txtOutstanding.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out outAmt);

            var supplier = new Supplier
            {
                SupplierCode = int.Parse(_txtCode.Text.Trim()),
                SupplierName = _txtName.Text.Trim(),
                Address = _txtAddress.Text.Trim(),
                Phone = _txtPhone.Text.Trim(),
                Gstin = _txtGstin.Text.Trim(),
                StateName = (_txtState.Text == null || _txtState.Text.Trim().Length == 0) ? "TAMILNADU" : _txtState.Text.Trim(),
                OutstandingAmount = outAmt
            };

            _repo.SaveSupplier(supplier, _isNewMode);
            RefreshGrid();
            if (DataChanged != null) DataChanged();
            MessageBox.Show("Supplier saved successfully.", "PharmaAid Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (_isNewMode) ResetForNew();
        }
    }
}
