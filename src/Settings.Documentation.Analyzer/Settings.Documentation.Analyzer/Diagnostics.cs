using Microsoft.CodeAnalysis;

namespace TomsToolbox.Settings.Documentation.Analyzer
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

        public static readonly DiagnosticDescriptor MissingDescriptionAttribute = new("CONF002",
            "Configuration property missing a description",
            "Property '{0}' in configuration options class '{1}' is missing a description",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All properties in configuration options classes should have a description for documentation generation.");

        public static readonly DiagnosticDescriptor MissingInvocatorAttribute = new("CONF003",
            "AddOptions invocator attribute missing",
            "AddOptions invocator {0} is missing the [SettingsOptionsInvocator] attribute",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Methods invoking the 'AddOptions<>' method should be decorated with the [SettingsOptionsInvocator] attribute, so they are discoverable."
            );
    }
}
