# TomsToolbox.Settings.Documentation

Automatically generate documentation for your .NET application configuration settings.

## Overview

Settings.Documentation is a runtime library that scans your configuration classes and generates comprehensive documentation in multiple formats:

- **JSON Schema** - Provides IntelliSense and validation in your `appsettings.json` files
- **Markdown** - Human-readable documentation of all configuration options
- **HTML** - Web-based documentation for easy sharing and viewing

The library can work with:
- Classes decorated with `[SettingsSection]` attribute
- Configuration options registered in `IServiceCollection`
- Any custom type mapping strategy

## Key Features

- Automatically discovers configuration classes in your assemblies
- Generates JSON schema files for IDE IntelliSense support
- Creates Markdown and HTML documentation
- Updates `appsettings.json` files with default values
- Respects `[SettingsIgnore]` and `[SettingsSecret]` attributes
- Configurable output formats and file locations
- Supports multiple target frameworks (.NET 8.0, .NET 10.0)

## Requirements

- .NET 8.0 or .NET 10.0
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.Options

## Related Packages

- **TomsToolbox.Settings.Documentation.Abstractions** - Attribute definitions for marking up configuration classes
- **TomsToolbox.Settings.Documentation.Analyzer** - Roslyn analyzer for compile-time validation

For detailed usage instructions, see the main repository documentation.

## License

MIT License

Part of the TomsToolbox library suite.
