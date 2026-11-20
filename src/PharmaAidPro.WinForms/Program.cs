using System;
using System.Windows.Forms;
using PharmaAidPro.WinForms.Data;
using PharmaAidPro.WinForms.Forms;

namespace PharmaAidPro.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseInitializer.Initialize();

            if (args != null && args.Length > 0 && args[0] == "--verify-db")
            {
                var repo = new PharmaRepository();
                var settings = repo.GetStoreSettings();
                var admin = repo.ValidateUser("Admin", "pass");
                int p, b, s;
                repo.GetDatabaseCounts(out p, out b, out s);
                string imgStatus = "NO_IMG";
                using (var bmp = LoadingSplashForm.LoadStoreBitmap())
                {
                    if (bmp != null)
                    {
                        using (var testCanvas = new System.Drawing.Bitmap(800, 600))
                        using (var g = System.Drawing.Graphics.FromImage(testCanvas))
                        {
                            g.DrawImage(bmp, 0, 0, 800, 600);
                        }
                        imgStatus = string.Format("IMG_OK_{0}x{1}", bmp.Width, bmp.Height);
                    }
                }
                Console.WriteLine("DB_OK|" + settings.StoreName + "|" + (admin != null ? admin.UserName : "NO_USER") + "|" + imgStatus + "|" + p + "|" + b + "|" + s);
                return;
            }

            var repository = new PharmaRepository();
            while (true)
            {
                string userName;
                string location;
                string finYear;

                using (var loginForm = new LoginForm(repository))
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        break;
                    }
                    userName = loginForm.LoggedInUser;
                    location = loginForm.SelectedLocation;
                    finYear = loginForm.SelectedFinYear;
                }

                using (var splash = new LoadingSplashForm(userName, location, finYear))
                {
                    splash.ShowDialog();
                }

                using (var mainForm = new MainForm(userName, finYear))
                {
                    Application.Run(mainForm);
                    if (!mainForm.LogoutRequested)
                    {
                        break;
                    }
                }
            }
        }
    }
}
