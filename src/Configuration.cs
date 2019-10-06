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
        private const bool DefaultLimitNotificationCount = false;

        /// <summary>
        /// Indicate if the number of notification to display is limited
        /// </summary>
        [DefaultValue(DefaultLimitNotificationCount)]
        public bool LimitNotificationCount { get; set; }

        /// <summary>
        /// Indicate whether to show the notifications
        /// </summary>
        [DefaultValue(DefaultShowHUD)]
        public bool ShowHUD { get; set; }

        /// <summary>
        /// Maximum number of notifications
        /// </summary>
        [DefaultValue(DefaultNotificationCount)]
        public int MaxNotificationCount { get; set; }

        public Configuration()
        {
            ShowHUD = DefaultShowHUD;
            MaxNotificationCount = DefaultNotificationCount;
            LimitNotificationCount = DefaultLimitNotificationCount;
        }

        public static Configuration Load()
        {
            if (File.Exists(LocalDataConfigurationFile))
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
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            using (StreamReader reader = new StreamReader(LocalDataConfigurationFile))
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
