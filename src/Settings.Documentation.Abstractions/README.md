# TomsToolbox.Settings.Documentation.Abstractions

Provides attributes for documenting and configuring .NET application settings classes.

## Overview

This package contains a set of attributes that help document configuration settings in .NET applications. These attributes work with configuration documentation generators to produce clear, structured documentation of your application's configuration options.

## Attributes

### `SettingsSectionAttribute`
Marks a class as settings class, and optionally specifies the configuration section name associated with the settings class.

```csharp
[SettingsSection("MyApp")]
public class MyAppSettings
{
    public string ApiUrl { get; set; }
}
```

### `SettingsIgnoreAttribute`
Excludes a class or property from settings documentation.

```csharp
public class Settings
{
    public string DocumentedProperty { get; set; }
    
    [SettingsIgnore]
    public string InternalProperty { get; set; }
}
```

### `SettingsSecretAttribute`
Marks properties containing sensitive data (passwords, API keys, etc.) to be masked in documentation.

```csharp
public class DatabaseSettings
{
    public string Server { get; set; }
    
    [SettingsSecret]
    public string Password { get; set; }
}
```

### `SettingsAddOptionsInvocatorAttribute`
Identifies methods that register configuration options with the dependency injection container.

```csharp
[SettingsAddOptionsInvocator]
public static void CustomAddOptions<T>(IServiceCollection services)
{
    services.AddOptions<T>().BindConfiguration(typeof(T).Name);
}
```

## Requirements

- .NET Standard 2.1 or higher

## License

MIT License

Part of the TomsToolbox library suite.
