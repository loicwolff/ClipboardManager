using System;
using System.Deployment.Application;
using System.IO;

namespace ClipboardManager
{
    public static class ClickOnceHelper
    {
        public static void AddShortcutToStartupGroup(string publisherName, string productName)
        {
            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                startupPath = Path.Combine(startupPath, productName) + ".appref-ms";

                if (!File.Exists(startupPath))
                {
                    string allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    
                    string shortcutPath = Path.Combine(allProgramsPath, publisherName, productName) + ".appref-ms";
                    if (File.Exists(shortcutPath))
                    {
                        File.Copy(shortcutPath, startupPath);
                    }
                }
            }
        }
    }
}
