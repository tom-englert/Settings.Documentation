# TomsToolbox.Settings.Documentation.Analyzer

Roslyn analyzer that helps ensure your configuration classes are properly annotated for automatic documentation generation.

## Overview

This analyzer provides compile-time diagnostics and code fixes to help you maintain consistent and complete documentation of your application's configuration settings. It works in conjunction with the Settings.Documentation library to ensure all configuration options are discoverable and well-documented.

## Diagnostics

### CONF001: Missing [SettingsSection] Attribute
**Severity:** Warning

Warns when a configuration options class used with `AddOptions<T>()` is missing the `[SettingsSection]` attribute.

**Code Fix:** Automatically adds the `[SettingsSection]` attribute to the class.

### CONF002: Missing Description
**Severity:** Warning

Warns when a property in a configuration options class lacks a `[Description]` attribute, which is used for documentation generation.

**Code Fix:** Automatically adds a `[Description]` attribute placeholder to the property.

### CONF003: Missing [SettingsAddOptionsInvocator] Attribute
**Severity:** Warning

Warns when a method that invokes `AddOptions<T>()` is missing the `[SettingsAddOptionsInvocator]` attribute, making it non-discoverable for documentation generation.

**Code Fix:** Automatically adds the `[SettingsAddOptionsInvocator]` attribute to the method.

## Installation

Install via NuGet:

```
dotnet add package TomsToolbox.Settings.Documentation.Analyzer
```

The analyzer is automatically integrated into your build process and provides real-time feedback in Visual Studio, VS Code, and other IDEs that support Roslyn analyzers.

## Requirements

- .NET Standard 2.0 or higher
- Microsoft.CodeAnalysis.CSharp 4.4.0 or higher

## Related Packages

- **TomsToolbox.Settings.Documentation.Abstractions** - Attribute definitions
- **TomsToolbox.Settings.Documentation.Builder** - Runtime library for generating documentation

For more information, see the main repository documentation.

## License

MIT License

Part of the TomsToolbox library suite.
