using System.Reflection;

namespace TomsToolbox.Settings.Documentation;

/// <summary>
/// Represents a configuration value associated with a specific section and property, including an optional default value.
/// </summary>
/// <param name="Section">The name of the configuration section that contains the value. Cannot be null or empty.</param>
/// <param name="Property">The property metadata that identifies the configuration value. Cannot be null.</param>
/// <param name="DefaultValue">The default value to use if the configuration value is not set. May be null.</param>
public record SettingsValue(string Section, PropertyInfo Property, string? DefaultValue);
