namespace TomsToolbox.Configuration.Documentation.Abstractions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SettingsSectionAttribute(string? sectionName = null) : Attribute
{
    public string? SectionName { get; } = sectionName;
}