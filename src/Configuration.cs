using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;

namespace ClipboardManager
{
    public class Configuration
    {
        public static string ConfigurationFile = "configuration.xml";

        private static string LocalDataFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clipboard Manager");
            }
        }

        private static string LocalDataConfigurationFile
        {
            get
            {
                return Path.Combine(LocalDataFolder, "configuration.xml");
            }
        }

        private const bool DefaultShowHUD = true;
        private const int DefaultNotificationCount = 5;
        private const bool DefaultAlwaysShowNotifications = true;
        private const bool DefaultLimitNotificationCount = false;

        /// <summary>
        /// Indique s'il faut limiter le nombre de notification à afficher
        /// </summary>
        public bool LimitNotificationCount { get; set; }

        /// <summary>
        /// Indique s'il faut afficher les notifications
        /// </summary>
        public bool ShowHUD { get; set; }

        /// <summary>
        /// Indique s'il faut toujours afficher les notifications (même quand le clipboard est identique)
        /// </summary>
        public bool AlwaysShowNotifications { get; set; }

        /// <summary>
        /// Nombre max de notification
        /// </summary>
        public int MaxNotificationCount { get; set; }

        public Configuration()
        {
            ShowHUD = DefaultShowHUD;
            MaxNotificationCount = DefaultNotificationCount;
            AlwaysShowNotifications = DefaultAlwaysShowNotifications;
            LimitNotificationCount = DefaultLimitNotificationCount;
        }

        public static Configuration Load()
        {
            if (File.Exists(ConfigurationFile))
            {
                Configuration configuration = LoadFromLegacyConfiguration();
                configuration.Save();

                File.Delete(ConfigurationFile);

                return configuration;
            }
            else if (File.Exists(LocalDataConfigurationFile))
            {
                return LoadFromConfigurationFile();
            }
            else
            {
                Configuration configuration = new Configuration();
                configuration.Save();
                return configuration;
            }
        }

        private static Configuration LoadFromConfigurationFile()
        {
            return LoadFromConfigurationCore(LocalDataConfigurationFile);
        }

        private static Configuration LoadFromLegacyConfiguration()
        {
            return LoadFromConfigurationCore(ConfigurationFile);
        }

        private static Configuration LoadFromConfigurationCore(string configurationFilePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            using (StreamReader reader = new StreamReader(configurationFilePath))
            {
                return serializer.Deserialize(reader) as Configuration;
            }
        }

        public void Save()
        {
            if (!Directory.Exists(LocalDataFolder))
            {
                Directory.CreateDirectory(LocalDataFolder);
            }

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            using (StreamWriter writer = new StreamWriter(LocalDataConfigurationFile))
            {
                serializer.Serialize(writer, this, ns);
            }
        }
    }
}
