using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmaAidPro.WinForms.Forms
{
    public class InlineSuggestPanel<T> : Panel where T : class
    {
        private readonly DataGridView _grid;
        private readonly Label _hintLabel;
        private List<T> _items = new List<T>();

        public event Action<T> ItemChosen;

        public InlineSuggestPanel(string hintText, params DataGridViewTextBoxColumn[] columns)
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Size = new Size(640, 220);
            this.Visible = false;

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
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                TabStop = false
            };

            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(46, 117, 182);
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 236, 245);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 8.5F, FontStyle.Bold);
            _grid.EnableHeadersVisualStyles = false;

            if (columns != null)
            {
                _grid.Columns.AddRange(columns);
            }

            _grid.CellDoubleClick += delegate
            {
                ConfirmCurrent();
            };

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                BackColor = Color.FromArgb(236, 240, 246)
            };
            _hintLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = hintText ?? "Up/Down: Move  |  Enter: Select  |  Esc: Close",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                Font = new Font("Tahoma", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 70, 85)
            };
            footer.Controls.Add(_hintLabel);

            this.Controls.Add(_grid);
            this.Controls.Add(footer);
        }

        public void ShowAt(Form hostForm, Point locationInHost, List<T> items)
        {
            _items = items ?? new List<T>();
            _grid.DataSource = null;
            _grid.DataSource = _items;

            if (_items.Count > 0 && _grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
            }

            int x = Math.Max(4, Math.Min(locationInHost.X, hostForm.ClientSize.Width - this.Width - 8));
            int y = locationInHost.Y;
            if (y + this.Height > hostForm.ClientSize.Height - 10)
            {
                y = Math.Max(4, locationInHost.Y - this.Height - 28);
            }

            this.Location = new Point(x, y);
            if (!this.Visible)
            {
                this.Visible = true;
            }
            this.BringToFront();
        }

        public void MoveSelection(int delta)
        {
            if (_grid.Rows.Count == 0) return;
            int current = _grid.CurrentRow != null ? _grid.CurrentRow.Index : 0;
            int next = Math.Max(0, Math.Min(_grid.Rows.Count - 1, current + delta));
            _grid.ClearSelection();
            _grid.Rows[next].Selected = true;
            _grid.CurrentCell = _grid.Rows[next].Cells[0];
            _grid.FirstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(next, _grid.Rows.Count - 1));
        }

        public T GetSelectedItem()
        {
            if (_grid.CurrentRow != null && _grid.CurrentRow.DataBoundItem is T)
            {
                return (T)_grid.CurrentRow.DataBoundItem;
            }
            return null;
        }

        public bool ConfirmCurrent()
        {
            var item = GetSelectedItem();
            if (item == null) return false;
            this.Visible = false;
            if (ItemChosen != null) ItemChosen(item);
            return true;
        }

        public void HidePopup()
        {
            this.Visible = false;
        }
    }
}
