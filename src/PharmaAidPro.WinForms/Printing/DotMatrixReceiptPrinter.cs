using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Printing
{
    public static class DotMatrixReceiptPrinter
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static string FormatReceiptText(StoreSettings store, SalesHeader header, List<SalesLine> lines)
        {
            var sb = new StringBuilder();
            const int width = 68;
            string lineSep = new string('-', width);

            sb.AppendLine(Center("WISH YOU A SPEEDY RECOVERY", width));
            sb.AppendLine();
            sb.AppendLine(Center(store.StoreName, width));
            sb.AppendLine(Center(store.Address, width));
            sb.AppendLine(string.Format("DLNo :{0,-32} GST No :{1}", Trunc(store.DrugLicenseNo, 31), store.Gstin));
            sb.AppendLine(lineSep);
            sb.AppendLine(string.Format("Name   : {0,-28} Bill No  : {1}", Trunc(header.CustomerName ?? string.Empty, 28), header.BillNo));
            sb.AppendLine(string.Format("Doctor : {0,-28} Bill Date: {1}", Trunc(header.DoctorName ?? string.Empty, 28), header.BillDate));
            sb.AppendLine(lineSep);
            sb.AppendLine(" Qty Particulars              Batch No   Exp     Rate     Amount");
            sb.AppendLine(lineSep);

            foreach (var item in lines)
            {
                string qtyStr = item.QtyUnits % 1 == 0
                    ? ((int)item.QtyUnits).ToString().PadLeft(4)
                    : item.QtyUnits.ToString("0.##").PadLeft(4);

                sb.AppendLine(string.Format(
                    "{0} {1,-24} {2,-9} {3,-6} {4,7:0.00} {5,9:0.00}",
                    qtyStr,
                    Trunc(item.ProductName, 24),
                    Trunc(item.BatchDesc, 9),
                    Trunc(item.ExpiryMMYY, 6),
                    item.RateMrp,
                    item.LineAmount));
            }

            sb.AppendLine(lineSep);
            sb.AppendLine(string.Format(
                "Base : {0:0.00} SGST: {1:0.00} CGST: {2:0.00} GST:{3:0.00}  ROD: {4:0.##}",
                header.BaseAmt, header.SgstAmt, header.CgstAmt, header.TotalGstAmt, header.RoundOff));
            sb.AppendLine(string.Format(
                "Total Item : {0}  Total Qty : {1:0.##}  User : {2}",
                header.TotalItems, header.TotalQty, header.CreatedBy));
            sb.AppendLine(string.Format("{0,68}", "Bill Amount : " + header.NetAmount.ToString("0.00")));
            sb.AppendLine(string.Format("{0,68}", "for " + store.StoreName));
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        public static bool SendRawTextToPrinter(string printerName, string text, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(printerName))
            {
                var ps = new PrinterSettings();
                printerName = ps.PrinterName;
            }

            if (string.IsNullOrWhiteSpace(printerName))
            {
                error = "No default printer found.";
                return false;
            }

            IntPtr pBytes = Marshal.StringToCoTaskMemAnsi(text);
            int dwCount = Encoding.ASCII.GetByteCount(text);
            IntPtr hPrinter = IntPtr.Zero;

            try
            {
                if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    error = "Could not open printer: " + printerName;
                    return false;
                }

                var di = new DOCINFOA { pDocName = "PharmaAidPro Receipt", pDataType = "RAW" };
                if (!StartDocPrinter(hPrinter, 1, di))
                {
                    error = "StartDocPrinter failed.";
                    return false;
                }

                if (!StartPagePrinter(hPrinter))
                {
                    error = "StartPagePrinter failed.";
                    EndDocPrinter(hPrinter);
                    return false;
                }

                int dwWritten;
                bool ok = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                if (!ok) error = "WritePrinter failed.";
                return ok;
            }
            finally
            {
                if (hPrinter != IntPtr.Zero) ClosePrinter(hPrinter);
                Marshal.FreeCoTaskMem(pBytes);
            }
        }

        public static void ShowPreviewDialog(IWin32Window owner, StoreSettings store, SalesHeader header, List<SalesLine> lines)
        {
            string receipt = FormatReceiptText(store, header, lines);

            using (var form = new Form())
            {
                form.Text = "Receipt Preview - " + header.BillNo;
                form.Size = new Size(640, 520);
                form.StartPosition = FormStartPosition.CenterParent;
                form.KeyPreview = true;

                var txt = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Consolas", 10F, FontStyle.Regular),
                    BackColor = Color.White,
                    Text = receipt.Replace("\n", Environment.NewLine)
                };

                var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(236, 240, 246) };
                var btnPrint = new Button { Text = "Print (TVS MSP / Default)", Left = 12, Top = 6, Width = 185, Height = 28 };
                btnPrint.Click += delegate
                {
                    string err;
                    if (SendRawTextToPrinter(store.PrinterName, receipt, out err))
                    {
                        MessageBox.Show("Sent to printer!", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Print warning: " + err, "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                var btnClose = new Button { Text = "Close (Esc)", Left = 515, Top = 6, Width = 95, Height = 28 };
                btnClose.Click += delegate { form.Close(); };

                footer.Controls.AddRange(new Control[] { btnPrint, btnClose });
                form.Controls.Add(txt);
                form.Controls.Add(footer);
                form.KeyDown += delegate(object s, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Escape) form.Close();
                };
                form.ShowDialog(owner);
            }
        }

        private static string Center(string s, int width)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Length >= width) return s.Substring(0, width);
            int left = (width - s.Length) / 2;
            return new string(' ', left) + s;
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
