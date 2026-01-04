namespace TomsToolbox.Settings.Documentation;

public class SettingsDocumentationBuilderOptions
{
    public string TargetDirectory { get; set; } = ".";

    public Func<Type, bool> TypeFilter { get; set; } = type => type.FullName?.StartsWith("Microsoft") != true;

    public Func<Type, string?> SectionMapping { get; set; } = type => null;

    public AppSettingsFileOptions[] AppSettingsFiles { get; set; } =
    [
        new() { FileName = "appsettings.json", UpdateDefaults = true },
        new() { FileName = "appsettings.Development.json", UpdateDefaults = false }
    ];

    public string AppSettingsSchemaFileName { get; set; } = "appsettings.schema.json";

    public bool GenerateSchema { get; set; } = true;

    public string AppSettingsMarkdownFileName { get; set; } = "appsettings.md";

    public bool GenerateMarkdown { get; set; } = true;

    public string AppSettingsHtmlFileName { get; set; } = "appsettings.html";

    public bool GenerateHtml { get; set; } = true;

    public bool ThrowOnUnknownSettingsSection { get; set; } = true;
}
