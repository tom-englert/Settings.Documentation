namespace TomsToolbox.Settings.Documentation.Builder;

/// <summary>
/// Represents the context information required to build documentation for settings.
/// </summary>
/// <param name="Values">An array settings values to include in documentation.</param>
/// <param name="Options">The options that control how the settings documentation is generated.</param>
public record SettingsDocumentationBuilderContext(IReadOnlyList<SettingsValue> Values, SettingsDocumentationBuilderOptions Options);
