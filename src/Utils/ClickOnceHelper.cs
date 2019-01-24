namespace ClipboardManager
{
    using System;
    using System.Deployment.Application;
    using System.IO;

    public static class ClickOnceHelper
    {
        public static void AddShortcutToStartupGroup(string publisherName, string productName)
        {
            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                string startupPath = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), productName)}.appref-ms";

                if (!File.Exists(startupPath))
                {
                    string shortcutPath = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), publisherName, productName)}.appref-ms";
                    
                    if (File.Exists(shortcutPath))
                    {
                        File.Copy(shortcutPath, startupPath);
                    }
                }
            }
        }
    }
}
