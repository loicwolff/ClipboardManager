using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace ClipboardManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ClickOnceHelper.AddShortcutToStartupGroup("Clipboard Manager", "Clipboard Manager");
            }
            catch (Exception)
            {
                //Trace.TraceWarning("Cannot add to startup group. Exception: {0}", ex.Message);
            }

            Application.Run(new TaskbarApplication());
        }
    }
}
