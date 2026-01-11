using System.ComponentModel.DataAnnotations;
using System.Reflection;

using TomsToolbox.Settings.Documentation.Abstractions;

namespace TomsToolbox.Settings.Documentation.Builder;

/// <summary>
/// Represents a configuration value associated with a specific section and property, including an optional default value.
/// </summary>
/// <param name="Section">The name of the configuration section that contains the value. Cannot be null or empty.</param>
/// <param name="Name">The name of the settings property.</param>
/// <param name="Description">The description of the settings value, typically derived from a DescriptionAttribute.</param>
/// <param name="ValueType">The type of the settings value.</param>
/// <param name="IsRequired">Indicates whether the value is required and must be provided.</param>
/// <param name="IsSecret">Indicates whether the value contains sensitive information and should be handled securely.</param>
/// <param name="IsIgnored">Indicates whether the value should be ignored during documentation generation.</param>
/// <param name="DefaultValue">The default value to use if the configuration value is not set. May be null.</param>
public record SettingsValue(string Section, string Name, string Description, Type ValueType, bool IsRequired, bool IsSecret, bool IsIgnored, object? DefaultValue)
{
    /// <summary>
    /// Creates a new <see cref="SettingsValue"/> instance from a property info and default instance.
    /// </summary>
    /// <param name="section">The name of the configuration section.</param>
    /// <param name="propertyInfo">The property info to extract settings information from.</param>
    /// <param name="defaultInstance">The default instance to retrieve the default value from.</param>
    /// <returns>A new <see cref="SettingsValue"/> instance with information extracted from the property.</returns>
    public static SettingsValue Create(string section, PropertyInfo propertyInfo, object defaultInstance)
    {
        var name = propertyInfo.Name;
        var description = propertyInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? string.Empty;
        var valueType = propertyInfo.PropertyType;
        var defaultValue = propertyInfo.GetValue(defaultInstance);
        var isSecret = propertyInfo.GetCustomAttribute<SettingsSecretAttribute>() is not null;
        var isIgnored = propertyInfo.GetCustomAttribute<SettingsIgnoreAttribute>() is not null;
        var isRequired = propertyInfo.GetCustomAttribute<RequiredAttribute>() is not null;

        return new(section, name, description, valueType, isRequired, isSecret, isIgnored, defaultValue);
    }
}
