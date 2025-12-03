using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace CodeTweakrsNetCleaner
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsRunAsAdmin())
            {
                RelaunchAsAdmin();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static bool IsRunAsAdmin()
        {
            using WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RelaunchAsAdmin()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show("Administrator rights are required.",
                    "CT-Cleaner Pro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
