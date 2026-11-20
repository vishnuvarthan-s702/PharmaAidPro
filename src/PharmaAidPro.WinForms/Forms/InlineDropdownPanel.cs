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

    public static class ExpiryMaskHelper
    {
        private static TextBox _attachedTextBox;

        public static void Attach(TextBox tb)
        {
            Detach();
            if (tb == null) return;

            _attachedTextBox = tb;
            tb.Text = NormalizeInitial(tb.Text);

            tb.KeyDown += Tb_KeyDown;
            tb.KeyPress += Tb_KeyPress;
            tb.MouseUp += Tb_MouseUp;

            // Override DataGridView's automatic SelectAll() so only the Month (MM) before '/' is selected
            tb.BeginInvoke((MethodInvoker)delegate
            {
                if (_attachedTextBox == tb && !tb.IsDisposed)
                {
                    int slash = tb.Text.IndexOf('/');
                    if (slash > 0)
                    {
                        tb.Select(0, slash);
                    }
                }
            });
        }

        public static void Detach()
        {
            if (_attachedTextBox != null)
            {
                _attachedTextBox.KeyDown -= Tb_KeyDown;
                _attachedTextBox.KeyPress -= Tb_KeyPress;
                _attachedTextBox.MouseUp -= Tb_MouseUp;
                _attachedTextBox = null;
            }
        }

        private static string NormalizeInitial(string raw)
        {
            string s = (raw ?? string.Empty).Trim().Replace("-", "/");
            if (s.Length == 0) return "12/28";
            int slash = s.IndexOf('/');
            if (slash < 0)
            {
                if (s.Length >= 4)
                {
                    return s.Substring(0, 2) + "/" + s.Substring(2, 2);
                }
                if (s.Length == 2)
                {
                    return s + "/28";
                }
                return "12/28";
            }
            string m = s.Substring(0, Math.Min(2, slash));
            string y = slash + 1 < s.Length ? s.Substring(slash + 1, Math.Min(2, s.Length - slash - 1)) : "28";
            if (m.Length == 1) m = "0" + m;
            if (m.Length == 0) m = "12";
            if (y.Length == 1) y = "0" + y;
            if (y.Length == 0) y = "28";
            return m + "/" + y;
        }

        private static void ClampSelectionAwayFromSlash(TextBox tb)
        {
            int slash = tb.Text.IndexOf('/');
            if (slash < 0) return;

            int start = tb.SelectionStart;
            int len = tb.SelectionLength;
            if (len > 0 && start <= slash && (start + len) > slash)
            {
                // Selection crosses '/' -> restrict selection strictly to the Month part (or Year part)
                if (start < slash)
                {
                    tb.Select(start, slash - start);
                }
                else
                {
                    tb.Select(slash + 1, tb.Text.Length - (slash + 1));
                }
            }
        }

        private static void Tb_MouseUp(object sender, MouseEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            ClampSelectionAwayFromSlash(tb);
        }

        private static void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            int slash = tb.Text.IndexOf('/');
            if (slash < 0)
            {
                tb.Text = NormalizeInitial(tb.Text);
                slash = tb.Text.IndexOf('/');
            }

            // Prevent Ctrl+A from selecting across the '/'
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                if (tb.SelectionStart <= slash)
                {
                    tb.Select(0, slash);
                }
                else
                {
                    tb.Select(slash + 1, tb.Text.Length - (slash + 1));
                }
                return;
            }

            ClampSelectionAwayFromSlash(tb);
            slash = tb.Text.IndexOf('/');

            if (e.KeyCode == Keys.Back)
            {
                if (tb.SelectionLength > 0)
                {
                    // Delete only the selected characters inside the current side (Month or Year)
                    int st = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                    tb.SelectionStart = st;
                    tb.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }

                // Never allow Backspace to delete the '/' separator
                if (tb.SelectionStart == slash + 1)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (tb.SelectionLength > 0)
                {
                    int st = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                    tb.SelectionStart = st;
                    tb.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }

                // Never allow Delete to remove the '/' separator
                if (tb.SelectionStart == slash)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }
        }

        private static void Tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            int slash = tb.Text.IndexOf('/');
            if (slash < 0) return;

            // Pressing '/' jumps directly from Month to Year selection
            if (e.KeyChar == '/')
            {
                e.Handled = true;
                tb.Select(slash + 1, tb.Text.Length - (slash + 1));
                return;
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            ClampSelectionAwayFromSlash(tb);
            slash = tb.Text.IndexOf('/');

            // Remove selected digits within the current side first
            if (tb.SelectionLength > 0)
            {
                int st = tb.SelectionStart;
                tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                tb.SelectionStart = st;
                tb.SelectionLength = 0;
                slash = tb.Text.IndexOf('/');
            }

            int pos = tb.SelectionStart;
            bool inMonth = pos <= slash;

            if (inMonth)
            {
                int monthLen = slash;
                if (monthLen < 2)
                {
                    // Insert digit into Month
                    tb.Text = tb.Text.Insert(pos, e.KeyChar.ToString());
                    tb.SelectionStart = pos + 1;
                    tb.SelectionLength = 0;
                }
                else
                {
                    // Month already has 2 digits: overtype at pos (or if at slash, jump to Year)
                    if (pos < 2)
                    {
                        tb.Text = tb.Text.Remove(pos, 1).Insert(pos, e.KeyChar.ToString());
                        tb.SelectionStart = pos + 1;
                        tb.SelectionLength = 0;
                    }
                    else
                    {
                        // Already typed 2 month digits and at '/' -> type into Year
                        int yearStart = slash + 1;
                        int yearLen = tb.Text.Length - yearStart;
                        if (yearLen > 0)
                        {
                            tb.Text = tb.Text.Substring(0, yearStart) + e.KeyChar.ToString();
                        }
                        else
                        {
                            tb.Text = tb.Text + e.KeyChar.ToString();
                        }
                        tb.SelectionStart = yearStart + 1;
                        tb.SelectionLength = 0;
                    }
                }
            }
            else
            {
                int yearStart = slash + 1;
                int yearLen = tb.Text.Length - yearStart;
                if (yearLen < 2)
                {
                    tb.Text = tb.Text.Insert(pos, e.KeyChar.ToString());
                    tb.SelectionStart = pos + 1;
                    tb.SelectionLength = 0;
                }
                else
                {
                    int targetPos = pos < tb.Text.Length ? pos : tb.Text.Length - 1;
                    tb.Text = tb.Text.Remove(targetPos, 1).Insert(targetPos, e.KeyChar.ToString());
                    tb.SelectionStart = Math.Min(tb.Text.Length, targetPos + 1);
                    tb.SelectionLength = 0;
                }
            }

            e.Handled = true;
        }
    }

    public static class FullDateMaskHelper
    {
        public static void AttachPermanent(TextBox tb)
        {
            if (tb == null) return;
            tb.Text = NormalizeDate(tb.Text);

            tb.Enter += delegate
            {
                tb.BeginInvoke((MethodInvoker)delegate
                {
                    if (!tb.IsDisposed)
                    {
                        SelectFirstSegment(tb);
                    }
                });
            };
            tb.MouseUp += Tb_MouseUp;
            tb.KeyDown += Tb_KeyDown;
            tb.KeyPress += Tb_KeyPress;
            tb.Leave += delegate
            {
                tb.Text = NormalizeDate(tb.Text);
            };
        }

        public static void SelectFirstSegment(TextBox tb)
        {
            if (tb == null) return;
            int s1 = tb.Text.IndexOf('/');
            if (s1 > 0)
            {
                tb.Select(0, s1);
            }
        }

        public static string NormalizeDate(string raw)
        {
            string today = DateTime.Now.ToString("dd/MM/yy");
            string s = (raw ?? string.Empty).Trim().Replace("-", "/");
            if (s.Length == 0) return today;

            string[] parts = s.Split('/');
            string[] defParts = today.Split('/');
            string d = parts.Length > 0 ? parts[0].Trim() : defParts[0];
            string m = parts.Length > 1 ? parts[1].Trim() : defParts[1];
            string y = parts.Length > 2 ? parts[2].Trim() : defParts[2];

            if (d.Length > 2) d = d.Substring(0, 2);
            if (m.Length > 2) m = m.Substring(0, 2);
            if (y.Length > 2) y = y.Substring(y.Length - 2, 2);

            if (d.Length == 1) d = "0" + d;
            if (d.Length == 0) d = defParts[0];
            if (m.Length == 1) m = "0" + m;
            if (m.Length == 0) m = defParts[1];
            if (y.Length == 1) y = "0" + y;
            if (y.Length == 0) y = defParts[2];

            return d + "/" + m + "/" + y;
        }

        private static void EnsureTwoSlashes(TextBox tb, out int s1, out int s2)
        {
            s1 = tb.Text.IndexOf('/');
            s2 = s1 >= 0 ? tb.Text.IndexOf('/', s1 + 1) : -1;
            if (s1 < 0 || s2 < 0)
            {
                tb.Text = NormalizeDate(tb.Text);
                s1 = tb.Text.IndexOf('/');
                s2 = tb.Text.IndexOf('/', s1 + 1);
            }
        }

        private static void GetSegmentBounds(TextBox tb, int caretPos, out int segIndex, out int segStart, out int segEnd)
        {
            int s1, s2;
            EnsureTwoSlashes(tb, out s1, out s2);

            if (caretPos <= s1)
            {
                segIndex = 0;
                segStart = 0;
                segEnd = s1;
            }
            else if (caretPos <= s2)
            {
                segIndex = 1;
                segStart = s1 + 1;
                segEnd = s2;
            }
            else
            {
                segIndex = 2;
                segStart = s2 + 1;
                segEnd = tb.Text.Length;
            }
        }

        private static void ClampSelectionToSingleSegment(TextBox tb)
        {
            if (tb.SelectionLength <= 0) return;
            int segIdx, segStart, segEnd;
            GetSegmentBounds(tb, tb.SelectionStart, out segIdx, out segStart, out segEnd);
            int selEnd = tb.SelectionStart + tb.SelectionLength;
            if (selEnd > segEnd)
            {
                int newLen = Math.Max(0, segEnd - tb.SelectionStart);
                tb.Select(tb.SelectionStart, newLen);
            }
        }

        private static void Tb_MouseUp(object sender, MouseEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            ClampSelectionToSingleSegment(tb);
        }

        private static void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            int s1, s2;
            EnsureTwoSlashes(tb, out s1, out s2);

            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                int segIdx, segStart, segEnd;
                GetSegmentBounds(tb, tb.SelectionStart, out segIdx, out segStart, out segEnd);
                tb.Select(segStart, segEnd - segStart);
                return;
            }

            ClampSelectionToSingleSegment(tb);
            EnsureTwoSlashes(tb, out s1, out s2);

            if (e.KeyCode == Keys.Back)
            {
                if (tb.SelectionLength > 0)
                {
                    int st = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                    tb.SelectionStart = st;
                    tb.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }

                // Block Backspace right after either '/' separator
                if (tb.SelectionStart == s1 + 1 || tb.SelectionStart == s2 + 1)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (tb.SelectionLength > 0)
                {
                    int st = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                    tb.SelectionStart = st;
                    tb.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }

                // Block Delete right before either '/' separator
                if (tb.SelectionStart == s1 || tb.SelectionStart == s2)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }
        }

        private static void Tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            int s1, s2;
            EnsureTwoSlashes(tb, out s1, out s2);

            // Pressing '/' jumps to the next segment (Day -> Month -> Year)
            if (e.KeyChar == '/')
            {
                e.Handled = true;
                int segIdx, segStart, segEnd;
                GetSegmentBounds(tb, tb.SelectionStart, out segIdx, out segStart, out segEnd);
                if (segIdx == 0)
                {
                    tb.Select(s1 + 1, s2 - (s1 + 1));
                }
                else if (segIdx == 1)
                {
                    tb.Select(s2 + 1, tb.Text.Length - (s2 + 1));
                }
                return;
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            ClampSelectionToSingleSegment(tb);
            if (tb.SelectionLength > 0)
            {
                int st = tb.SelectionStart;
                tb.Text = tb.Text.Remove(st, tb.SelectionLength);
                tb.SelectionStart = st;
                tb.SelectionLength = 0;
            }

            EnsureTwoSlashes(tb, out s1, out s2);
            int pos = tb.SelectionStart;
            int curSeg, curStart, curEnd;
            GetSegmentBounds(tb, pos, out curSeg, out curStart, out curEnd);
            int segLen = curEnd - curStart;

            if (segLen < 2)
            {
                tb.Text = tb.Text.Insert(pos, e.KeyChar.ToString());
                tb.SelectionStart = pos + 1;
                tb.SelectionLength = 0;
            }
            else
            {
                if (pos < curEnd)
                {
                    tb.Text = tb.Text.Remove(pos, 1).Insert(pos, e.KeyChar.ToString());
                    tb.SelectionStart = pos + 1;
                    tb.SelectionLength = 0;
                }
                else if (curSeg < 2)
                {
                    // Current segment is full (2 digits) and caret is at '/' -> type into next segment
                    int nextStart = curEnd + 1;
                    int nextEnd = curSeg == 0 ? s2 : tb.Text.Length;
                    tb.Text = tb.Text.Remove(nextStart, nextEnd - nextStart).Insert(nextStart, e.KeyChar.ToString());
                    tb.SelectionStart = nextStart + 1;
                    tb.SelectionLength = 0;
                }
                else
                {
                    // At the end of Year segment -> overtype last digit
                    int lastIdx = tb.Text.Length - 1;
                    tb.Text = tb.Text.Remove(lastIdx, 1) + e.KeyChar.ToString();
                    tb.SelectionStart = tb.Text.Length;
                    tb.SelectionLength = 0;
                }
            }

            e.Handled = true;
        }
    }
}
