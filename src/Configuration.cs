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
        public const string ConfigurationFile = "configuration.xml";

        private static readonly string LocalDataConfigurationFile = Path.Combine(TaskbarApplication.LocalDataFolder, ConfigurationFile);

        private const bool DefaultShowHUD = true;
        private const int DefaultNotificationCount = 5;
        private const bool DefaultAlwaysShowNotifications = true;
        private const bool DefaultLimitNotificationCount = false;

        /// <summary>
        /// Indique s'il faut limiter le nombre de notification à afficher
        /// </summary>
        [DefaultValue(DefaultLimitNotificationCount)]
        public bool LimitNotificationCount { get; set; }

        /// <summary>
        /// Indique s'il faut afficher les notifications
        /// </summary>
        [DefaultValue(DefaultShowHUD)]
        public bool ShowHUD { get; set; }

        /// <summary>
        /// Indique s'il faut toujours afficher les notifications (même quand le clipboard est identique)
        /// </summary>
        [DefaultValue(DefaultAlwaysShowNotifications)]
        public bool AlwaysShowNotifications { get; set; }

        /// <summary>
        /// Nombre max de notification
        /// </summary>
        [DefaultValue(DefaultNotificationCount)]
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

        private static Configuration LoadFromConfigurationFile() => LoadFromConfigurationCore(LocalDataConfigurationFile);

        private static Configuration LoadFromLegacyConfiguration() => LoadFromConfigurationCore(ConfigurationFile);

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
