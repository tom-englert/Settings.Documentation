using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TomsToolbox.Settings.Documentation.Abstractions;

namespace TomsToolbox.Settings.Documentation.Builder;

using static System.Web.HttpUtility;

/// <summary>
/// Provides functionality to generate documentation for application settings from configuration classes.
/// </summary>
public static class SettingsDocumentation
{
    private static readonly HashSet<Type> NumberTypes = [typeof(int), typeof(short), typeof(double), typeof(float)];
    private static readonly JsonSerializerOptions JsonSerializerOptions = CreateSerializerOptions();

    /// <summary>
    /// Creates a settings documentation builder context from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for configuration classes marked with <see cref="SettingsSectionAttribute"/>.</param>
    /// <param name="configureOptions">An optional action to configure the documentation builder options.</param>
    /// <returns>A <see cref="SettingsDocumentationBuilderContext"/> containing the discovered settings and configured options.</returns>
    /// <remarks>
    /// This method scans all types in the provided assemblies and includes those decorated with <see cref="SettingsSectionAttribute"/>.
    /// </remarks>
    public static SettingsDocumentationBuilderContext SettingsDocumentationBuilder(Assembly[] assemblies, Action<SettingsDocumentationBuilderOptions>? configureOptions = null)
    {
        var types = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttribute<SettingsSectionAttribute>() != null)
            .ToArray();

        return SettingsDocumentationBuilder(types, configureOptions);
    }

    /// <summary>
    /// Creates a settings documentation builder context from configuration options registered in a service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection containing registered configuration options.</param>
    /// <param name="configureOptions">An optional action to configure the documentation builder options.</param>
    /// <returns>A <see cref="SettingsDocumentationBuilderContext"/> containing the discovered settings and configured options.</returns>
    /// <remarks>
    /// This method extracts configuration types from services registered as <see cref="IConfigureOptions{TOptions}"/> in the service collection.
    /// </remarks>
    public static SettingsDocumentationBuilderContext SettingsDocumentationBuilder(this IServiceCollection serviceCollection, Action<SettingsDocumentationBuilderOptions>? configureOptions = null)
    {
        var optionsTypeName = typeof(IConfigureOptions<>).Name;

        var configuredOptions = serviceCollection
            .Where(descriptor => descriptor.ServiceType.Name == optionsTypeName)
            .ToArray();

        var types = configuredOptions.Select(sd => sd.ServiceType.GenericTypeArguments[0])
            .Distinct()
            .ToArray();

        return SettingsDocumentationBuilder(types, configureOptions);
    }

    /// <summary>
    /// Creates a settings documentation builder context from an array of configuration types.
    /// </summary>
    /// <param name="types">The array of types representing configuration classes to document.</param>
    /// <param name="configureOptions">An optional action to configure the documentation builder options.</param>
    /// <returns>A <see cref="SettingsDocumentationBuilderContext"/> containing the extracted settings and configured options.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a configuration section cannot be determined for a type and 
    /// <see cref="SettingsDocumentationBuilderOptions.ThrowOnUnknownSettingsSection"/> is <c>true</c>.
    /// </exception>
    /// <remarks>
    /// This method processes each type to extract its properties as configuration settings. Properties marked with 
    /// <see cref="SettingsIgnoreAttribute"/> are excluded. Types marked with <see cref="SettingsIgnoreAttribute"/> 
    /// are also excluded. The method applies the configured type filter and section mapping.
    /// </remarks>
    public static SettingsDocumentationBuilderContext SettingsDocumentationBuilder(Type[] types, Action<SettingsDocumentationBuilderOptions>? configureOptions = null)
    {
        var values = new List<SettingsValue>();

        var options = new SettingsDocumentationBuilderOptions();

        configureOptions?.Invoke(options);

        foreach (var configClass in types)
        {
            if (configClass.GetCustomAttribute<SettingsIgnoreAttribute>() is not null)
                continue;

            var section = options.SectionMapping(configClass) ?? configClass.GetSection();

            if (string.IsNullOrEmpty(section))
            {
                if (options.TypeFilter(configClass) && options.ThrowOnUnknownSettingsSection)
                {
                    throw new InvalidOperationException($"Unable to get the section for {configClass}; Define a custom section mapping, exclude this type or switch of {nameof(SettingsDocumentationBuilderOptions.ThrowOnUnknownSettingsSection)}");
                }

                continue;
            }

            var defaultInstance = Activator.CreateInstance(configClass);

            var configurationValues = configClass
                .GetProperties()
                .Where(propertyInfo => propertyInfo is { CanRead: true, CanWrite: true })
                .Select(propertyInfo => new SettingsValue(section, propertyInfo, Convert.ToString(propertyInfo.GetValue(defaultInstance), CultureInfo.InvariantCulture)))
                .Where(item => !item.IsIgnored());

            values.AddRange(configurationValues);
        }

        return new(values, options);
    }

    /// <summary>
    /// Generates all configured documentation files from the builder context.
    /// </summary>
    /// <param name="context">The settings documentation builder context containing values and options.</param>
    /// <remarks>
    /// <para>
    /// This method generates the following files based on the configuration:
    /// </para>
    /// <list type="bullet">
    /// <item><description>JSON schema file (if <see cref="SettingsDocumentationBuilderOptions.GenerateSchema"/> is <c>true</c>)</description></item>
    /// <item><description>Markdown documentation file (if <see cref="SettingsDocumentationBuilderOptions.GenerateMarkdown"/> is <c>true</c>)</description></item>
    /// <item><description>HTML documentation file (if <see cref="SettingsDocumentationBuilderOptions.GenerateHtml"/> is <c>true</c>)</description></item>
    /// <item><description>Updated appsettings files with schema references and default values</description></item>
    /// </list>
    /// <para>
    /// All files are written to the directory specified by <see cref="SettingsDocumentationBuilderOptions.TargetDirectory"/>.
    /// The target directory is created if it does not exist.
    /// </para>
    /// </remarks>
    public static void GenerateDocumentation(this SettingsDocumentationBuilderContext context)
    {
        var options = context.Options;
        var targetDirectory = options.TargetDirectory;
        var valuesBySection = context.Values.GroupBy(value => value.Section).ToArray();


        Directory.CreateDirectory(targetDirectory);

        if (options.GenerateSchema)
        {
            File.WriteAllText(Path.Combine(targetDirectory, options.AppSettingsMarkdownFileName), CreateMarkdownDocumentation(valuesBySection));
        }

        if (options.GenerateHtml)
        {
            File.WriteAllText(Path.Combine(targetDirectory, options.AppSettingsHtmlFileName), CreateHtmlDocumentation(valuesBySection));
        }

        if (options.GenerateMarkdown)
        {
            File.WriteAllText(Path.Combine(targetDirectory, options.AppSettingsSchemaFileName), CreateSchema(valuesBySection));
        }

        foreach (var appSettingsFile in options.AppSettingsFiles)
        {
            var appSettingsPath = Path.Combine(targetDirectory, appSettingsFile.FileName);
            var appSettings = ReadAppSettings(appSettingsPath) ?? [];

            if (UpdateSettings(valuesBySection, appSettings, options.AppSettingsSchemaFileName, appSettingsFile.UpdateDefaults, options.GenerateSchema))
            {
                File.WriteAllText(appSettingsPath, appSettings.ToJsonString(JsonSerializerOptions));
            }
        }
    }

    private static bool UpdateSettings(IEnumerable<IGrouping<string, SettingsValue>> valuesBySection, JsonNode appSettings, string appSettingsSchemaFileName, bool updateDefaults, bool updateSchema)
    {
        var sectionsUpdated = 0;

        if (updateSchema)
        {
            var schemaFileName = $"./{appSettingsSchemaFileName}";

            if (!string.Equals(appSettings["$schema"]?.ToString(), schemaFileName, StringComparison.OrdinalIgnoreCase))
            {
                appSettings["$schema"] = schemaFileName;
                sectionsUpdated++;
            }
        }

        if (updateDefaults)
        {
            foreach (var sectionValues in valuesBySection)
            {
                var section = sectionValues.Key;
                var sectionNode = appSettings[section] ?? new JsonObject();
                var valuesAdded = 0;

                foreach (var sectionValue in sectionValues)
                {
                    var (_, property, defaultValue) = sectionValue;
                    var name = property.Name;

                    if (sectionNode[name] != null)
                        continue; // don't override existing settings

                    if (sectionValue.IsSecret())
                        defaultValue = "*****";

                    ++valuesAdded;

                    sectionNode[name] = GetJsonValue(defaultValue, property.PropertyType);
                }

                if (valuesAdded <= 0)
                    continue;

                appSettings[section] = sectionNode;
                sectionsUpdated++;
            }
        }

        return sectionsUpdated > 0;
    }

    private static string CreateSchema(IEnumerable<IGrouping<string, SettingsValue>> valuesBySection)
    {
        const string schemaScaffold = """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "$id": "https://tom-englert.de/schema/custom-appsettings.json",
          "additionalProperties": true,
          "properties": { "$schema": { "type": "string" } }
        }
        """;

        // ! template is a constant value, it will never fail.
        var schema = (JsonObject)JsonNode.Parse(schemaScaffold)!;
        // ! template is a constant value, it will never fail.
        var sectionsNode = (JsonObject)schema["properties"]!;

        foreach (var valueSection in valuesBySection)
        {
            var section = valueSection.Key;

            var sectionNode = new JsonObject();

            sectionsNode.Add(section, sectionNode);

            var propertiesNode = new JsonObject();

            sectionNode.Add("additionalProperties", JsonValue.Create(false));
            sectionNode.Add("properties", propertiesNode);

            AddSchemaProperties(valueSection, propertiesNode);
        }

        return schema.ToJsonString(JsonSerializerOptions);
    }

    private static void AddSchemaProperties(IEnumerable<SettingsValue> values, JsonObject sectionNode)
    {
        foreach (var value in values)
        {
            var property = value.Property;
            var name = property.Name;
            var propertyType = property.PropertyType;
            var settingsType = propertyType.GetSettingsTypeName();

            var typeNode = new JsonObject();

            sectionNode.Add(name, typeNode);

            typeNode.Add("type", JsonValue.Create(new[] { settingsType, "null" }));

            if (propertyType.IsEnum)
            {
                typeNode.Add("enum", JsonValue.Create(Enum.GetNames(propertyType)));
            }

            var description = property.GetDescription();
            if (!string.IsNullOrEmpty(description))
            {
                typeNode.Add("description", JsonValue.Create(description));
            }

            var defaultValue = value.IsSecret() ? "*****" : value.DefaultValue;

            typeNode.Add("default", GetJsonValue(defaultValue, property.PropertyType));
        }
    }

    private static JsonValue? GetJsonValue(string? value, Type propertyType)
    {
        if (propertyType == typeof(bool))
        {
            return JsonValue.Create(bool.TryParse(value, out var b) && b);
        }

        if (NumberTypes.Contains(propertyType) && double.TryParse(value, out var d))
        {
            return JsonValue.Create(d);
        }

        return JsonValue.Create(value);
    }

    static void AddMarkdownPropertyDocumentation(IEnumerable<SettingsValue> values, StringBuilder text)
    {
        foreach (var value in values)
        {
            var property = value.Property;
            var name = property.Name;
            var propertyType = property.PropertyType;
            var settingsType = propertyType.GetSettingsTypeName();

            text.AppendLine($"### {name}")
                .AppendLine($"  - type: {settingsType}");

            if (propertyType.IsEnum)
            {
                text.AppendLine($"  - values: {string.Join(", ", Enum.GetNames(propertyType))}");
            }

            var defaultValue = value.IsSecret() ? "*****" : value.DefaultValue;
            text.AppendLine($"  - default: {defaultValue}");

            var description = property.GetDescription();
            if (!string.IsNullOrEmpty(description))
            {
                text.AppendLine($"  - description: {description}");
            }
        }
    }

    private static string CreateMarkdownDocumentation(IEnumerable<IGrouping<string, SettingsValue>> valuesBySection)
    {
        var text = new StringBuilder()
            .AppendLine("# Configuration Settings");

        foreach (var sectionValues in valuesBySection)
        {
            var section = sectionValues.Key;
            var values = sectionValues.ToArray();

            if (values.Length == 0)
                continue;

            text.AppendLine($"## {section}");

            AddMarkdownPropertyDocumentation(values, text);
        }

        return text.ToString();
    }

    static void AddHtmlPropertyDocumentation(IEnumerable<SettingsValue> configurationValues, StringBuilder text)
    {
        foreach (var value in configurationValues)
        {
            var property = value.Property;
            var key = property.Name;
            var propertyType = property.PropertyType;
            var settingsType = propertyType.GetSettingsTypeName();


            text.AppendLine($"<h4>{HtmlEncode(key)}</h4>")
                .AppendLine("<ul>")
                .AppendLine($"<li>type: {HtmlEncode(settingsType)}</li>");

            if (propertyType.IsEnum)
            {
                text.AppendLine($"<li>values: {HtmlEncode(string.Join(", ", Enum.GetNames(propertyType)))}</li>");
            }

            var defaultValue = value.IsSecret() ? "*****" : value.DefaultValue;
            text.AppendLine($"<li>default: {HtmlEncode(defaultValue)}</li>");

            var description = property.GetDescription();
            if (!string.IsNullOrEmpty(description))
            {
                text.AppendLine($"<li>description: {HtmlEncode(description)}</li>");
            }

            text.AppendLine("</ul>");
        }
    }

    private static string CreateHtmlDocumentation(IEnumerable<IGrouping<string, SettingsValue>> valuesBySection)
    {
        const string header = """
                              <!DOCTYPE html>
                              <html lang="en">
                              <head>
                                  <title>Vita Configuration Settings</title>
                                  <style>
                                  body { font-family: sans-serif; } 
                                  h3 { margin-left: 1em; }
                                  h4 { margin: 0 0 0 2em; }
                                  ul { margin-left: 1em; }
                                  li { padding: 0.2em; }
                                  </style>
                              </head>
                              <body>
                              
                              """;

        var text = new StringBuilder(header)
            .AppendLine("<h2>Configuration Settings</h2>");

        foreach (var valueSection in valuesBySection)
        {
            var section = valueSection.Key;
            var values = valueSection.ToArray();

            if (values.Length == 0)
                continue;

            text.AppendLine($"<h3>{HtmlEncode(section)}</h3>");

            AddHtmlPropertyDocumentation(values, text);
        }

        text.AppendLine("</body>")
            .AppendLine("</html>")
            .AppendLine("");

        return text.ToString();
    }

    private static string? GetSection(this Type type)
    {
        var sectionAttribute = type.GetCustomAttribute<SettingsSectionAttribute>();

        if (sectionAttribute is null)
            return type.GetSectionFromField();

        return sectionAttribute.SectionName ?? type.GetSectionFromField() ?? type.Name;
    }

    private static string? GetSectionFromField(this Type type)
    {
        var field = type.GetField("ConfigurationSection", BindingFlags.Public | BindingFlags.Static);
        return field?.GetValue(null) as string;
    }

    private static string? GetDescription(this PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    private static string GetSettingsTypeName(this Type propertyType)
    {
        if (NumberTypes.Contains(propertyType))
        {
            return propertyType == typeof(bool) ? "boolean" : "number";
        }

        return propertyType == typeof(bool) ? "boolean" : "string";
    }

    private static bool IsSecret(this SettingsValue value)
    {
        return value.Property.GetCustomAttribute<SettingsSecretAttribute>() != null;
    }

    private static bool IsIgnored(this SettingsValue value)
    {
        return value.Property.GetCustomAttribute<SettingsIgnoreAttribute>() != null;
    }

    private static JsonObject? ReadAppSettings(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            }
        }
        catch
        {
            // unable to load/parse file, go with default
        }

        return null;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions() { WriteIndented = true };

        options.MakeReadOnly(true);

        return options;
    }
}
