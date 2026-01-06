namespace TomsToolbox.Settings.Documentation.Builder;

/// <summary>
/// Configuration options for the settings documentation builder that controls how documentation files are generated
/// from application settings classes.
/// </summary>
public class SettingsDocumentationBuilderOptions
{
    /// <summary>
    /// Gets or sets the target directory where generated documentation files will be written.
    /// Default is "." (current directory).
    /// </summary>
    public string TargetDirectory { get; set; } = ".";

    /// <summary>
    /// Gets or sets a filter function that determines which types should be included in the documentation.
    /// Default filters out types whose full name starts with "Microsoft".
    /// </summary>
    /// <remarks>
    /// The function receives a <see cref="Type"/> and returns <c>true</c> if the type should be included
    /// in the documentation, or <c>false</c> to exclude it.
    /// </remarks>
    public Func<Type, bool> TypeFilter { get; set; } = type => type.FullName?.StartsWith("Microsoft") != true;

    /// <summary>
    /// Gets or sets a function that maps a settings type to a custom configuration section name.
    /// Default returns <c>null</c> to use the section name defined by the type's attributes.
    /// </summary>
    /// <remarks>
    /// The function receives a <see cref="Type"/> and returns a custom section name, or <c>null</c>
    /// to use the default section name from the type's metadata.
    /// </remarks>
    public Func<Type, string?> SectionMapping { get; set; } = type => null;

    /// <summary>
    /// Gets or sets the collection of appsettings files to process.
    /// Default includes "appsettings.json" with defaults updated and "appsettings.Development.json" without defaults.
    /// </summary>
    public AppSettingsFileOptions[] AppSettingsFiles { get; set; } =
    [
        new() { FileName = "appsettings.json", UpdateDefaults = true },
        new() { FileName = "appsettings.Development.json", UpdateDefaults = false }
    ];

    /// <summary>
    /// Gets or sets the filename for the generated JSON schema file.
    /// Default is "appsettings.schema.json".
    /// </summary>
    public string AppSettingsSchemaFileName { get; set; } = "appsettings.schema.json";

    /// <summary>
    /// Gets or sets a value indicating whether to generate a JSON schema file.
    /// Default is <c>true</c>.
    /// </summary>
    public bool GenerateSchema { get; set; } = true;

    /// <summary>
    /// Gets or sets the filename for the generated Markdown documentation file.
    /// Default is "appsettings.md".
    /// </summary>
    public string AppSettingsMarkdownFileName { get; set; } = "appsettings.md";

    /// <summary>
    /// Gets or sets a value indicating whether to generate a Markdown documentation file.
    /// Default is <c>true</c>.
    /// </summary>
    public bool GenerateMarkdown { get; set; } = true;

    /// <summary>
    /// Gets or sets the filename for the generated HTML documentation file.
    /// Default is "appsettings.html".
    /// </summary>
    public string AppSettingsHtmlFileName { get; set; } = "appsettings.html";

    /// <summary>
    /// Gets or sets a value indicating whether to generate an HTML documentation file.
    /// Default is <c>true</c>.
    /// </summary>
    public bool GenerateHtml { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception when an unknown settings section is encountered
    /// in the appsettings files.
    /// Default is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the builder will throw an exception if it finds a configuration class where it cannot determine the corresponding section name.
    /// When <c>false</c>, unknown sections are ignored.
    /// </remarks>
    public bool ThrowOnUnknownSettingsSection { get; set; } = true;
}
