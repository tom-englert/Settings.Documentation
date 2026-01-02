namespace TomsToolbox.Configuration.Documentation.Abstractions;

/// <summary>
/// Specifies the configuration section name associated with a settings class.
/// </summary>
/// <remarks>Apply this attribute to a class to indicate which configuration section it should be bound to when loading settings. This is typically used in applications that support configuration binding, such as those using Microsoft.Extensions.Configuration.</remarks>
/// <param name="sectionName">The name of the configuration section to associate with the class. If null or not specified, a default section name may be used.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SettingsSectionAttribute(string? sectionName = null) : Attribute
{
    public string? SectionName { get; } = sectionName;
}
