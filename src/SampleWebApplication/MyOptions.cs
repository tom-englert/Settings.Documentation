using System.ComponentModel;

using TomsToolbox.Configuration.Documentation.Abstractions;

namespace TomsToolbox.SampleWebApplication;

[SettingsSection]
public class MyOptions
{
    public const string ConfigurationSection = nameof(MyOptions);

    [Description("The port the service is running on")]
    public int Port { get; init; } = 99;
    [Description("The host ulr running the service")]
    public string Host { get; init; } = "localhost";
}
