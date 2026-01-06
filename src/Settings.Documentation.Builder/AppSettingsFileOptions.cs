namespace TomsToolbox.Settings.Documentation.Builder;

/// <summary>
/// Represents the options for processing an application settings file.
/// </summary>
public class AppSettingsFileOptions
{
    /// <summary>
    /// Gets or sets the name of the application settings file to process, e.g. 'appsettings.json' or 'appsettings.Development.json'
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add missing default values in the settings file.
    /// </summary>
    public bool UpdateDefaults { get; set; }
}
