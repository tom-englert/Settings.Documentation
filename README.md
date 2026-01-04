# Settings.Documentation

[![NuGet](https://img.shields.io/nuget/v/Settings.Documentation.svg)](https://www.nuget.org/packages/Settings.Documentation/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Automatically generate comprehensive documentation for your .NET application configuration settings. Keep your `appsettings.json` files documented, validated, and always up-to-date with JSON schemas, Markdown, and HTML documentation.

## 🎯 Overview

Settings.Documentation is a toolset that helps you maintain clear, accurate documentation of your application's configuration options. It consists of three main packages that work together to provide compile-time validation and runtime documentation generation:

- **Settings.Documentation.Abstractions** - Attributes for marking up configuration classes
- **Settings.Documentation** - Runtime library for generating documentation
- **Settings.Documentation.Analyzer** - Roslyn analyzer for compile-time validation

## ✨ Features

- 🔍 **JSON Schema Generation** - Automatic IntelliSense and validation in your `appsettings.json` files
- 📝 **Markdown Documentation** - Human-readable documentation of all configuration options
- 🌐 **HTML Documentation** - Web-based documentation for easy sharing
- ⚙️ **Automatic Discovery** - Finds configuration classes via attributes or DI registration
- 🔒 **Secret Protection** - Masks sensitive values in documentation
- 🛠️ **Compile-Time Validation** - Roslyn analyzer ensures all settings are properly documented
- 🚀 **Multi-Framework Support** - Works with .NET 8.0, .NET 10.0, and .NET Standard 2.1

## 📦 Installation

Install the packages via NuGet:

```bash
# Core library for documentation generation
dotnet add package Settings.Documentation

# Attributes for marking up configuration classes
dotnet add package Settings.Documentation.Abstractions

# Optional: Roslyn analyzer for compile-time validation
dotnet add package Settings.Documentation.Analyzer
```

## 🚀 Quick Start

### 1. Mark Your Configuration Classes

Use attributes to document your settings:

```csharp
using System.ComponentModel;
using TomsToolbox.Settings.Documentation.Abstractions;

[SettingsSection]
public class MyAppSettings
{
    public const string ConfigurationSection = nameof(MyAppSettings);

    [Description("The port the service is running on")]
    public int Port { get; init; } = 8080;

    [Description("The host URL running the service")]
    public string Host { get; init; } = "localhost";
}

[SettingsSection]
public class DatabaseSettings
{
    [Description("The database connection string")]
    public string ConnectionString { get; init; } = "Server=.;Database=MyDb;";

    [Description("The database password")]
    [SettingsSecret]  // Masks the value in documentation
    public string Password { get; init; } = "";
}
```

### 2. Register in Dependency Injection

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MyAppSettings>()
    .BindConfiguration(MyAppSettings.ConfigurationSection);

builder.Services
    .AddOptions<DatabaseSettings>()
    .BindConfiguration("ConnectionStrings");
```

### 3. Generate Documentation

Create a console application to generate documentation:

```csharp
using TomsToolbox.Settings.Documentation;

var builder = WebApplication.CreateBuilder(args);

// Configure your services as usual
ConfigureServices(builder.Services);

// Generate documentation
builder.Services
    .SettingsDocumentationBuilder(options =>
    {
        options.TargetDirectory = "./docs";
        options.GenerateSchema = true;
        options.GenerateMarkdown = true;
        options.GenerateHtml = true;
    })
    .GenerateDocumentation();
```

### 4. Build and Run

The documentation generator will create:

- `appsettings.schema.json` - JSON schema for IntelliSense
- `appsettings.md` - Markdown documentation
- `appsettings.html` - HTML documentation
- Updated `appsettings.json` with schema reference and default values

## 📖 Available Attributes

### `[SettingsSection]`
Marks a class as a configuration section.

```csharp
[SettingsSection]
public class MySettings { }
```

### `[Description]`
Provides documentation for a property (uses `System.ComponentModel.DescriptionAttribute`).

```csharp
[Description("The timeout in seconds")]
public int Timeout { get; set; }
```

### `[SettingsSecret]`
Marks a property as containing sensitive data (will be masked as `*****` in documentation).

```csharp
[SettingsSecret]
public string ApiKey { get; set; }
```

### `[SettingsIgnore]`
Excludes a property or class from documentation.

```csharp
[SettingsIgnore]
public string InternalProperty { get; set; }
```

### `[SettingsAddOptionsInvocator]`
Marks methods that register configuration options for analyzer discovery.

```csharp
[SettingsAddOptionsInvocator]
public static void ConfigureServices(IServiceCollection services)
{
    services.AddOptions<MySettings>();
}
```

## 🔧 Configuration Options

Customize the documentation generation:

```csharp
builder.Services
    .SettingsDocumentationBuilder(options =>
    {
        // Output directory
        options.TargetDirectory = "./docs";
        
        // Control what gets generated
        options.GenerateSchema = true;
        options.GenerateMarkdown = true;
        options.GenerateHtml = true;
        
        // File names
        options.AppSettingsSchemaFileName = "appsettings.schema.json";
        options.AppSettingsMarkdownFileName = "appsettings.md";
        options.AppSettingsHtmlFileName = "appsettings.html";
        
        // Which appsettings files to update
        options.AppSettingsFiles = new[]
        {
            new AppSettingsFileOptions 
            { 
                FileName = "appsettings.json", 
                UpdateDefaults = true 
            },
            new AppSettingsFileOptions 
            { 
                FileName = "appsettings.Development.json", 
                UpdateDefaults = false 
            }
        };
        
        // Custom type filtering
        options.TypeFilter = type => type.FullName?.StartsWith("MyApp") == true;
        
        // Custom section mapping
        options.SectionMapping = type => type.Name.Replace("Settings", "");
        
        // Validation
        options.ThrowOnUnknownSettingsSection = true;
    })
    .GenerateDocumentation();
```

## 📍 Section Name Resolution

The library uses a flexible system to determine the configuration section name for each settings class. The section name is resolved in the following priority order:

### 1. Explicit Section Name in Attribute

When you provide a section name directly in the `[SettingsSection]` attribute:

```csharp
[SettingsSection("MyCustomSection")]
public class MyAppSettings
{
    public int Port { get; set; }
}
```

This maps to:
```json
{
  "MyCustomSection": {
    "Port": 8080
  }
}
```

### 2. ConfigurationSection Constant

If the attribute has no explicit name, the library looks for a public static `ConfigurationSection` field:

```csharp
[SettingsSection]
public class MyAppSettings
{
    public const string ConfigurationSection = "MyApp";

    public int Port { get; set; }
}
```

This maps to:
```json
{
  "MyApp": {
    "Port": 8080
  }
}
```

### 3. Class Name (Default)

If neither of the above is found, the class name itself is used:

```csharp
[SettingsSection]
public class DatabaseSettings
{
    public string ConnectionString { get; set; }
}
```

This maps to:
```json
{
  "DatabaseSettings": {
    "ConnectionString": "..."
  }
}
```

### 4. Custom Section Mapping

You can also provide a custom function to determine section names:

```csharp
builder.Services
    .SettingsDocumentationBuilder(options =>
    {
        // Remove "Settings" suffix from class names
        options.SectionMapping = type => 
            type.Name.EndsWith("Settings") 
                ? type.Name[..^8]  // Remove last 8 characters
                : type.Name;
    })
    .GenerateDocumentation();
```

With this mapping:
```csharp
[SettingsSection]
public class DatabaseSettings { }  // Maps to "Database" section
```

### Resolution Priority Summary

The section name is resolved in this order:

1. **Custom mapping**: `options.SectionMapping(type)`
2. **Attribute parameter**: `[SettingsSection("ExplicitName")]`
3. **ConfigurationSection field**: `public const string ConfigurationSection = "FieldName";`
4. **Class name**: Uses the type's `Name`, if a `[SettingsSection]` attribure is present


### Handling Unknown Sections

If no section name can be determined, the behavior depends on the `ThrowOnUnknownSettingsSection` option:

```csharp
options.ThrowOnUnknownSettingsSection = true;  // Default: throws exception
options.ThrowOnUnknownSettingsSection = false; // Silently skips the type
```

## 🔍 Roslyn Analyzer

The analyzer provides compile-time diagnostics:

| Code | Severity | Description |
|------|----------|-------------|
| CONF001 | Warning | Configuration class missing `[SettingsSection]` attribute |
| CONF002 | Warning | Property missing `[Description]` attribute |
| CONF003 | Warning | AddOptions invocator missing `[SettingsAddOptionsInvocator]` attribute |

All diagnostics include automatic code fixes!

## 🏗️ Integration with Build Process

Add to your `.csproj` to automatically update documentation on build:

```xml
<Target Name="UpdateDocumentation" AfterTargets="Build">
  <Message Importance="high" Text="Updating documentation files..." />
  <Exec Command="dotnet &quot;$(TargetPath)&quot;" WorkingDirectory="$(ProjectDir)" />
</Target>
```

## 📂 Project Structure

```
Configuration.Documentation/
├── Settings.Documentation.Abstractions/    # Attribute definitions
├── Settings.Documentation/                 # Core documentation library
├── Settings.Documentation.Analyzer/        # Roslyn analyzer
│   ├── Settings.Documentation.Analyzer/           # Analyzer implementation
│   ├── Settings.Documentation.Analyzer.CodeFixes/ # Code fix providers
│   ├── Settings.Documentation.Analyzer.Package/   # NuGet package
│   └── Settings.Documentation.Analyzer.Test/      # Unit tests
├── SampleWebApplication/                   # Example web application
└── SampleDocumentationBuilder/            # Example documentation builder
```

## 🎨 Example Output

### JSON Schema
Your `appsettings.json` gets IntelliSense:

```json
{
  "$schema": "./appsettings.schema.json",
  "MyAppSettings": {
    "Port": 8080,
    "Host": "localhost"
  }
}
```

### Markdown Documentation
Clean, readable documentation:

```markdown
# Application Configuration

## MyAppSettings
- **Port** (number): The port the service is running on
  - Default: `8080`
- **Host** (string): The host URL running the service
  - Default: `"localhost"`
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## 📄 License

This project is part of the TomsToolbox library suite.

## 🔗 Related Projects

Part of the [TomsToolbox](https://github.com/tom-englert/TomsToolbox) collection of useful utilities for .NET development.

---

**Made with** 🔧📖 **by the TomsToolbox team**
