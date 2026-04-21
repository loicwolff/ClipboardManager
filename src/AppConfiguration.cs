using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ClipboardManager;

public class AppConfiguration
{
    public const string ConfigurationFile = "configuration.xml";

    private static readonly string LocalDataConfigurationFile = Path.Combine(TaskbarApplication.LocalDataFolder, ConfigurationFile);

    private const bool DefaultShowHUD = true;
    private const int DefaultNotificationCount = 5;
    private const bool DefaultLimitNotificationCount = false;

    /// <summary>
    /// Indicates whether the number of notifications to display is limited.
    /// </summary>
    [DefaultValue(DefaultLimitNotificationCount)]
    public bool LimitNotificationCount { get; set; }

    /// <summary>
    /// Indicates whether to show notifications.
    /// </summary>
    [DefaultValue(DefaultShowHUD)]
    public bool ShowHUD { get; set; }

    /// <summary>
    /// Maximum number of notifications.
    /// </summary>
    [DefaultValue(DefaultNotificationCount)]
    public int MaxNotificationCount { get; set; }

    public AppConfiguration()
    {
        this.ShowHUD = DefaultShowHUD;
        this.MaxNotificationCount = DefaultNotificationCount;
        this.LimitNotificationCount = DefaultLimitNotificationCount;
    }

    public static AppConfiguration Load()
    {
        if (File.Exists(LocalDataConfigurationFile))
        {
            return LoadFromConfigurationFile();
        }
        else
        {
            AppConfiguration configuration = new();
            configuration.Save();
            return configuration;
        }
    }

    private static AppConfiguration LoadFromConfigurationFile()
    {
        XmlSerializer serializer = new(typeof(AppConfiguration));

        using StreamReader reader = new(LocalDataConfigurationFile);
        using XmlReader xmlReader = XmlReader.Create(reader);
        return serializer.Deserialize(xmlReader) as AppConfiguration ?? new AppConfiguration();
    }

    public void Save()
    {
        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer serializer = new(typeof(AppConfiguration));

        using StreamWriter writer = new(LocalDataConfigurationFile);
        serializer.Serialize(writer, this, ns);
    }
}
