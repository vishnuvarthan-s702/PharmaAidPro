using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Forms
{
    public class HsnSelectorForm : Form
    {
        private readonly DataGridView _grid;
        private readonly List<HsnTax> _items;
        public HsnTax SelectedHsn { get; private set; }

        public HsnSelectorForm(PharmaRepository repo)
        {
            _items = repo.GetAllHsnTaxes();

            this.Text = "Select HSN Code & GST %";
            this.Size = new Size(560, 340);
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
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HsnCode", HeaderText = "HSN Code", FillWeight = 22 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", FillWeight = 48 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TaxPercent", HeaderText = "GST %", FillWeight = 14 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TaxLabel", HeaderText = "Tax Slab", FillWeight = 16 });
            _grid.AutoGenerateColumns = false;
            _grid.DataSource = _items;

            _grid.CellDoubleClick += delegate { ConfirmSelection(); };
            _grid.KeyDown += Grid_KeyDown;

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = Color.FromArgb(236, 240, 246) };
            var hint = new Label
            {
                Text = "Up/Down: Select  |  Enter: Confirm  |  Esc: Cancel",
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 360,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            var okBtn = new Button { Text = "Select", Width = 80, Height = 26, Left = 385, Top = 5 };
            okBtn.Click += delegate { ConfirmSelection(); };
            var cancelBtn = new Button { Text = "Close", Width = 75, Height = 26, Left = 470, Top = 5 };
            cancelBtn.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };

            footer.Controls.AddRange(new Control[] { hint, okBtn, cancelBtn });
            this.Controls.Add(_grid);
            this.Controls.Add(footer);

            this.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ConfirmSelection();
            }
        }

        private void ConfirmSelection()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is HsnTax)
            {
                SelectedHsn = (HsnTax)_grid.CurrentRow.DataBoundItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
