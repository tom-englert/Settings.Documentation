using Microsoft.CodeAnalysis;

namespace TomsToolbox.Configuration.Documentation.Analyzer
{
    public static class Diagnostics
    {
        private const string Category = "Configuration";

        public static readonly DiagnosticDescriptor MissingSettingsSectionAttribute = new("CONF001",
            "Configuration options class missing [SettingsSection] attribute",
            "Type '{0}' used in AddOptions<T>() is missing the [SettingsSection] attribute",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Configuration options classes registered with AddOptions<T>() should be marked with [SettingsSection] attribute for documentation generation.");

        public static readonly DiagnosticDescriptor ConfigurationHasNoDescription = new("CONF002",
            "Configuration property missing description",
            "Property '{0}' in configuration options class '{1}' is missing a description",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All properties in configuration options classes should have a description for documentation generation.");
    }
}
