using System.Reflection;

namespace TomsToolbox.Configuration.Documentation;

public record ConfigurationValue(string Section, PropertyInfo Property, object? DefaultValue);