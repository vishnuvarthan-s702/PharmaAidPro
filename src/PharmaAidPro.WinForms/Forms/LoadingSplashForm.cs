using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace PharmaAidPro.WinForms.Forms
{
    public class LoadingSplashForm : Form
    {
        private readonly string _userName;
        private readonly string _location;
        private readonly string _finYear;
        private Image _bgImage;
        private Image _logoImage;
        private Timer _timer;
        private int _progress;
        private string _statusMessage;

        public LoadingSplashForm(string userName, string location, string finYear)
        {
            _userName = userName ?? "Admin";
            _location = location ?? "MAIN STORE";
            _finYear = finYear ?? "2026 - 2027";
            _progress = 0;
            _statusMessage = string.Format("Authenticating {0} ({1})...", _userName, _location);

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(760, 540);
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(16, 36, 62);
            this.KeyPreview = true;
            ApplyBrandingIcon(this);

            LoadStoreBackgroundImage();

            _timer = new Timer { Interval = 30 };
            _timer.Tick += Timer_Tick;

            this.Shown += delegate { _timer.Start(); };
            this.FormClosed += delegate
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
                if (_bgImage != null)
                {
                    _bgImage.Dispose();
                    _bgImage = null;
                }
                if (_logoImage != null)
                {
                    _logoImage.Dispose();
                    _logoImage = null;
                }
            };
        }

        public static void ApplyBrandingIcon(Form form)
        {
            if (form == null) return;
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string icoPath = Path.Combine(Path.Combine(baseDir, "Assets"), "app.ico");
                if (File.Exists(icoPath))
                {
                    form.Icon = new Icon(icoPath);
                    return;
                }
                form.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
            }
        }

        public static string ResolveLogoPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(Path.Combine(baseDir, "Assets"), "app_logo.jpg"),
                Path.Combine(baseDir, "app_logo.jpg"),
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Assets\app_logo.jpg"))
            };
            foreach (var path in candidates)
            {
                if (File.Exists(path)) return path;
            }
            return null;
        }

        public static Bitmap LoadLogoBitmap()
        {
            try
            {
                string path = ResolveLogoPath();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    using (var ms = new MemoryStream(bytes))
                    using (var tempImg = Image.FromStream(ms))
                    {
                        var bmp = new Bitmap(tempImg.Width, tempImg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(tempImg, 0, 0, tempImg.Width, tempImg.Height);
                        }
                        return bmp;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public static string ResolveStoreBackgroundPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(Path.Combine(baseDir, "Assets"), "store_bg.jpg"),
                Path.Combine(baseDir, "store_bg.jpg"),
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Assets\store_bg.jpg"))
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        public static Bitmap LoadStoreBitmap()
        {
            try
            {
                string path = ResolveStoreBackgroundPath();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    using (var ms = new MemoryStream(bytes))
                    using (var tempImg = Image.FromStream(ms))
                    {
                        var bmp = new Bitmap(tempImg.Width, tempImg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(tempImg, 0, 0, tempImg.Width, tempImg.Height);
                        }
                        return bmp;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private void LoadStoreBackgroundImage()
        {
            _bgImage = LoadStoreBitmap();
            _logoImage = LoadLogoBitmap();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _progress += 2;

            if (_progress < 30)
            {
                _statusMessage = string.Format("Authenticating User '{0}' at {1}...", _userName, _location);
            }
            else if (_progress < 65)
            {
                _statusMessage = "Loading Medicine Catalog, HSN Slabs & Batch Inventory...";
            }
            else if (_progress < 90)
            {
                _statusMessage = string.Format("Initializing Retail Billing Counter (FY {0})...", _finYear);
            }
            else
            {
                _statusMessage = "Welcome to The Nethaji Medical Stores!";
            }

            this.Invalidate();

            if (_progress >= 100)
            {
                _timer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int footerHeight = 112;

            // Draw Store Photo in the Background
            var photoRect = new Rectangle(0, 0, w, h - footerHeight);
            if (_bgImage != null)
            {
                g.DrawImage(_bgImage, photoRect);
            }
            else
            {
                using (var br = new LinearGradientBrush(photoRect, Color.FromArgb(20, 58, 100), Color.FromArgb(10, 28, 52), 90f))
                {
                    g.FillRectangle(br, photoRect);
                }
            }

            // Top translucent banner
            using (var topBrush = new SolidBrush(Color.FromArgb(175, 12, 32, 58)))
            {
                g.FillRectangle(topBrush, 0, 0, w, 38);
            }
            using (var badgeFont = new Font("Tahoma", 9.5F, FontStyle.Bold))
            {
                g.DrawString("PharmaAid Pro  •  Retail & Wholesale Pharmacy ERP (v7.5.16)", badgeFont, Brushes.White, 16, 10);
                string fyText = string.Format("User: {0}  |  FY: {1}", _userName, _finYear);
                var sz = g.MeasureString(fyText, badgeFont);
                g.DrawString(fyText, badgeFont, Brushes.Khaki, w - sz.Width - 16, 10);
            }

            // Bottom Footer Overlay Panel
            var footerRect = new Rectangle(0, h - footerHeight, w, footerHeight);
            using (var footerBrush = new LinearGradientBrush(footerRect, Color.FromArgb(245, 16, 44, 78), Color.FromArgb(255, 10, 26, 48), 90f))
            {
                g.FillRectangle(footerBrush, footerRect);
            }

            int textX = 24;
            if (_logoImage != null)
            {
                var logoRect = new Rectangle(16, h - footerHeight + 10, 92, 92);
                g.DrawImage(_logoImage, logoRect);
                using (var goldPen = new Pen(Color.FromArgb(255, 216, 102), 1.5f))
                {
                    g.DrawRectangle(goldPen, logoRect);
                }
                textX = 122;
            }

            using (var titleFont = new Font("Tahoma", 15F, FontStyle.Bold))
            using (var subFont = new Font("Tahoma", 8.75F, FontStyle.Bold))
            using (var statusFont = new Font("Tahoma", 9F, FontStyle.Regular))
            using (var pctFont = new Font("Tahoma", 9.5F, FontStyle.Bold))
            using (var goldBrush = new SolidBrush(Color.FromArgb(255, 216, 102)))
            using (var lightBlueBrush = new SolidBrush(Color.FromArgb(205, 228, 252)))
            {
                g.DrawString("The Nethaji Medical Stores", titleFont, Brushes.White, textX, h - footerHeight + 10);
                g.DrawString("CHEMIST & DRUGGIST  •  No.204/A, Meenakshipuram, Nagercoil - 629001", subFont, goldBrush, textX + 2, h - footerHeight + 37);

                // Status & Percentage
                g.DrawString(_statusMessage, statusFont, lightBlueBrush, textX + 2, h - footerHeight + 60);
                string pctText = string.Format("{0}%", Math.Min(100, _progress));
                var pctSize = g.MeasureString(pctText, pctFont);
                g.DrawString(pctText, pctFont, Brushes.White, w - pctSize.Width - 24, h - footerHeight + 58);

                // Progress Bar Track
                var barTrack = new Rectangle(textX + 2, h - 26, w - (textX + 26), 14);
                using (var trackBrush = new SolidBrush(Color.FromArgb(38, 65, 98)))
                using (var trackPen = new Pen(Color.FromArgb(80, 120, 165)))
                {
                    g.FillRectangle(trackBrush, barTrack);
                    g.DrawRectangle(trackPen, barTrack);
                }

                // Progress Bar Fill
                int fillWidth = (int)Math.Round((barTrack.Width - 2) * (Math.Min(100, _progress) / 100.0));
                if (fillWidth > 0)
                {
                    var fillRect = new Rectangle(barTrack.X + 1, barTrack.Y + 1, fillWidth, barTrack.Height - 2);
                    using (var fillBrush = new LinearGradientBrush(fillRect, Color.FromArgb(46, 204, 113), Color.FromArgb(26, 160, 85), 0f))
                    {
                        g.FillRectangle(fillBrush, fillRect);
                    }
                }
            }

            // Outer Frame Border
            using (var borderPen = new Pen(Color.FromArgb(65, 115, 170), 2f))
            {
                g.DrawRectangle(borderPen, 1, 1, w - 2, h - 2);
            }
        }
    }
}
