using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomsToolbox.Settings.Documentation.Abstractions;

namespace TomsToolbox.SampleWebApplication;

[SettingsSection]
public class MyOptions
{
    public const string ConfigurationSection = nameof(MyOptions);

    [Description("The port the service is running on")]
    public int Port { get; init; } = 99;

    [Description("The host ulr running the service")]
    public string Host { get; init; } = "localhost";

    [Description("The supported cultures")]
    public IReadOnlyCollection<string> SupportedCultures { get; init; } = ["en-US", "de-DE"];

    [Description("Enable verbose logging")]
    public bool EnableVerboseLogging { get; init; } = false;

    [Description("The timeout for service calls")]
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    [Description("Database connection strings")]
    public string? Optional { get; set; } = null;

    [Description("A required setting without default value")]
    [Required]
    // ! null assertion for demonstration purposes
    public string Required { get; set; } = null!;

    [Description("Special chars")]
    public string? SpecialChars { get; set; } = "<SomeSpecialChars'$%&>";
}

[SettingsSection]
public class DatabaseConnectionStrings
{
    public const string ConfigurationSection = "ConnectionStrings";

    [Description("The database connection string")]
    public string Database { get; init; } = "Server=.;Database=MyDb;Trusted_Connection=True;";

    [Description("The database users password")]
    [SettingsSecret]
    public string Password { get; init; } = "SecretPassword";
}

[SettingsSection]
public class MessageQueueConnectionStrings
{
    public const string ConfigurationSection = "ConnectionStrings";

    [Description("The message queue connection string")]
    public string MessageQueue { get; init; } = "Server=.;Queue=MyQueue";

    [SettingsIgnore]
    public string UndocumentedFeature { get; init; } = "Not documented";
}
