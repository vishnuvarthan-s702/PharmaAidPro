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
                var admin = repo.ValidateUser("Admin", "SHOPAID");
                int p, b, s;
                repo.GetDatabaseCounts(out p, out b, out s);
                Console.WriteLine("DB_OK|" + settings.StoreName + "|" + (admin != null ? admin.UserName : "NO_USER") + "|" + p + "|" + b + "|" + s);
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
