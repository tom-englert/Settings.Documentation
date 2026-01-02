namespace TomsToolbox.Configuration.Documentation.Abstractions;

/// <summary>
/// Indicates that a property contains sensitive or secret configuration data that should be represented as '*****' in the documentation.
/// </summary>
/// <remarks>Apply this attribute to properties that store secrets, such as passwords, API keys, or connection strings, to signal that they require special handling</remarks>
[AttributeUsage(AttributeTargets.Property)]
public class SettingsSecretAttribute : Attribute
{
}
