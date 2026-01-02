namespace TomsToolbox.Configuration.Documentation.Abstractions;

/// <summary>
/// Indicates that a class or property should be excluded from settings documentation.
/// </summary>
/// <remarks>Apply this attribute to a class or property to prevent it from being included from the generated documentation.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class SettingsIgnoreAttribute : Attribute
{
}
