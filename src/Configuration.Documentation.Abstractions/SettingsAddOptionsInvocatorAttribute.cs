namespace TomsToolbox.Configuration.Documentation.Abstractions;

/// <summary>
/// Indicates that a method is used to invoke the AddOptions method for adding configuration options.
/// </summary>
/// <remarks>Apply this attribute to methods that serve as invocators for adding options.</remarks>
[AttributeUsage(AttributeTargets.Method)]
public class SettingsAddOptionsInvocatorAttribute : Attribute
{
}
