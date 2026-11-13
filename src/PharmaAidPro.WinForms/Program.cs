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
                int p, b, s;
                repo.GetDatabaseCounts(out p, out b, out s);
                Console.WriteLine("DB_OK|" + settings.StoreName + "|" + p + "|" + b + "|" + s);
                return;
            }

            Application.Run(new MainForm());
        }
    }
}
