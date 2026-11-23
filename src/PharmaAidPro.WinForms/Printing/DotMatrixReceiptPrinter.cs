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
            return BuildReceiptInternal(store, header, lines, false);
        }

        public static string FormatEscPRawReceipt(StoreSettings store, SalesHeader header, List<SalesLine> lines)
        {
            return BuildReceiptInternal(store, header, lines, true);
        }

        private static string BuildReceiptInternal(StoreSettings store, SalesHeader header, List<SalesLine> lines, bool includeEscP)
        {
            var sb = new StringBuilder();
            const int width = 60;
            string margin = "  ";
            string lineSep = margin + new string('-', width);

            // ESC @ (Reset), ESC 2 (1/6 inch line spacing), SI (0x0F = 17.1 CPI Condensed Mode ON)
            if (includeEscP)
            {
                sb.Append("\x1B@\x1B2\x0F");
            }

            // Top margin below perforation
            sb.Append("\r\n\r\n");

            string storeTitle = (store.StoreName ?? "THE NETHAJI MEDICAL STORES").ToUpperInvariant();
            if (includeEscP)
            {
                // Print Store Title in 10 CPI wide mode (0x12 = DC2), centered across 36 columns, then switch back to 17 CPI Condensed (0x0F = SI)
                sb.Append("\x12");
                sb.Append(Center(storeTitle, 36));
                sb.Append("\x0F\r\n");
            }
            else
            {
                sb.Append(margin + Center(storeTitle, width) + "\r\n");
            }

            sb.Append(margin + Center(store.Address ?? "No.204/A,Meenakshipuram,Nagercoil-629001", width) + "\r\n");
            sb.Append(string.Format("{0}DLNo :{1,-29} GST No :{2}\r\n", margin, Trunc(store.DrugLicenseNo, 29), store.Gstin));
            sb.Append(lineSep + "\r\n");
            sb.Append(string.Format("{0}Name   :{1,-24} Bill No  :{2}\r\n", margin, Trunc(header.CustomerName ?? string.Empty, 24), header.BillNo));
            sb.Append(string.Format("{0}{1,-32} Bill Date:{2}\r\n", margin, string.Empty, header.BillDate));
            sb.Append(string.Format("{0}Doctor :{1}\r\n", margin, Trunc(header.DoctorName ?? string.Empty, 24)));
            sb.Append(lineSep + "\r\n");
            sb.Append(margin + " Qty Particulars            Batch No  Exp     Rate    Amount\r\n");
            sb.Append(lineSep + "\r\n");

            int printedLines = 0;
            foreach (var item in lines)
            {
                string qtyStr = item.QtyUnits % 1 == 0
                    ? ((int)item.QtyUnits).ToString().PadLeft(4)
                    : item.QtyUnits.ToString("0.##").PadLeft(4);

                sb.Append(string.Format(
                    "{0}{1} {2,-22} {3,-9} {4,-5} {5,7:0.00} {6,8:0.00}\r\n",
                    margin,
                    qtyStr,
                    Trunc(item.ProductName, 22),
                    Trunc(item.BatchDesc, 9),
                    Trunc(item.ExpiryMMYY, 5),
                    item.RateMrp,
                    item.LineAmount));
                printedLines++;
            }

            // Pad item area to at least 2 lines so single-item bills stay compact without a large empty gap
            const int minItemLines = 2;
            while (printedLines < minItemLines)
            {
                sb.Append("\r\n");
                printedLines++;
            }

            sb.Append(lineSep + "\r\n");

            // Row 1 below divider: Base / SGST / CGST neatly on the left; bold Bill Amount on the right under Amount column
            string taxLine1 = string.Format(
                "Base:{0:0.00} SGST:{1:0.00} CGST:{2:0.00}",
                header.BaseAmt, header.SgstAmt, header.CgstAmt);
            if (taxLine1.Length > 37) taxLine1 = taxLine1.Substring(0, 37);
            taxLine1 = taxLine1.PadRight(37);

            string billAmtBlock = string.Format("Bill Amount : {0:0.00}", header.NetAmount).PadLeft(23);
            if (includeEscP)
            {
                // ESC E (\x1BE) turns Bold ON without changing 17-CPI character width; ESC F (\x1BF) turns Bold OFF
                sb.Append(string.Format("{0}{1}\x1BE{2}\x1BF\r\n", margin, taxLine1, billAmtBlock));
            }
            else
            {
                sb.Append(string.Format("{0}{1}{2}\r\n", margin, taxLine1, billAmtBlock));
            }

            // Row 2 below divider: GST, ROD, Items, and Qty all grouped neatly on the left (leaving space below Bill Amount empty)
            string taxLine2 = string.Format(
                "GST :{0:0.00}  ROD:{1:0.00}  Items:{2}  Qty:{3:0.##}",
                header.TotalGstAmt, header.RoundOff, header.TotalItems, header.TotalQty);
            sb.Append(margin + taxLine2 + "\r\n");

            // Row 3 below divider: Store signature right-aligned
            sb.Append(string.Format("{0}{1,60}\r\n", margin, "for " + storeTitle));
            sb.Append(lineSep + "\r\n");
            sb.Append(margin + Center("WISH YOU A SPEEDY RECOVERY", width) + "\r\n");
            sb.Append(margin + Center("THANKYOU", width) + "\r\n");

            // Feed 8 blank lines so THANKYOU advances past the printer's tear-off blade (which sits ~8 lines above the print head)
            sb.Append("\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");
            return sb.ToString();
        }

        public static bool SendRawTextToPrinter(string printerName, string text, out string error)
        {
            error = null;
            if (printerName == null || printerName.Trim().Length == 0)
            {
                var ps = new PrinterSettings();
                printerName = ps.PrinterName;
            }

            if (printerName == null || printerName.Trim().Length == 0)
            {
                error = "No default printer found.";
                return false;
            }

            if (text != null && !text.StartsWith("\x1B@", StringComparison.Ordinal))
            {
                text = "\x1B@\x1B2\x0F" + text;
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
                    string escPText = FormatEscPRawReceipt(store, header, lines);
                    if (SendRawTextToPrinter(store.PrinterName, escPText, out err))
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
