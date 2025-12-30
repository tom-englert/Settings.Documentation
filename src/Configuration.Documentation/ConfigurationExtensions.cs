using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TomsToolbox.Configuration.Documentation.Abstractions;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace TomsToolbox.Configuration.Documentation;

using static System.Web.HttpUtility;

public static class ConfigurationExtensions
{
    private static readonly HashSet<Type> NumberTypes = [typeof(int), typeof(short), typeof(double), typeof(float)];
    private static readonly JsonSerializerOptions JsonSerializerOptions = CreateSerializerOptions();

    private const string AppSettingsFileName = "appsettings.json";
    private const string AppSettingsSchemaFileName = "appsettings.schema.json";

    public static void BuildConfigurationDocumentation(this IServiceCollection serviceCollection)
    {
        var optionsTypeName = typeof(IConfigureOptions<>).Name;

        var configureOptions = serviceCollection
            .Where(descriptor => descriptor.ServiceType.Name == optionsTypeName)
            .ToArray();

        var values = new List<ConfigurationValue>();

        foreach (var serviceDescriptor in configureOptions)
        {
            var configClass = serviceDescriptor.ServiceType.GenericTypeArguments[0];
            if (configClass.GetCustomAttribute<SettingsIgnoreAttribute>() is not null)
                continue;

            var section = configClass.GetSection();

            if (string.IsNullOrEmpty(section))
                continue;

            var defaultInstance = Activator.CreateInstance(configClass);

            var configurationValues = configClass
                .GetProperties()
                .Where(propertyInfo => propertyInfo is { CanRead: true, CanWrite: true })
                .Select(propertyInfo => new ConfigurationValue(section, propertyInfo, Convert.ToString(propertyInfo.GetValue(defaultInstance), CultureInfo.InvariantCulture)))
                .Where(item => !item.IsIgnored());

            values.AddRange(configurationValues);
        }

        var valuesBySection = values.GroupBy(value => value.Section).ToArray();

        var appSettingsFolder = @"D:\dev\GitHub\ConfigurationDocs\SampleWebApplication\";
        var appSettingsPath = Path.Combine(appSettingsFolder, AppSettingsFileName);

        var appSettings = ReadAppSettings(appSettingsPath) ?? new JsonObject();

        if (UpdateSettings(valuesBySection, appSettings))
        {
            File.WriteAllText(appSettingsPath, appSettings.ToJsonString(JsonSerializerOptions));
        }

        File.WriteAllText(Path.Combine(appSettingsFolder, "appsettings.md"), CreateMarkdownDocumentation(valuesBySection));
        File.WriteAllText(Path.Combine(appSettingsFolder, "appsettings.html"), CreateHtmlDocumentation(valuesBySection));
        File.WriteAllText(Path.Combine(appSettingsFolder, AppSettingsSchemaFileName), CreateSchema(valuesBySection));
    }

    private static bool UpdateSettings(IEnumerable<IGrouping<string, ConfigurationValue>> valuesBySection, JsonNode appSettings)
    {
        var sectionsUpdated = 0;

        const string schemaFileName = $"./{AppSettingsSchemaFileName}";

        if (!string.Equals(appSettings["$schema"]?.ToString(), schemaFileName, StringComparison.OrdinalIgnoreCase))
        {
            appSettings["$schema"] = schemaFileName;
            sectionsUpdated++;
        }

        foreach (var sectionValues in valuesBySection)
        {
            var section = sectionValues.Key;
            var sectionNode = appSettings[section] ?? new JsonObject();
            var valuesAdded = 0;

            foreach (var (_, property, defaultValue) in sectionValues)
            {
                var name = property.Name;

                if (sectionNode[name] != null)
                    continue; // don't override existing settings

                ++valuesAdded;

                sectionNode[name] = GetJsonValue(defaultValue, property.PropertyType);
            }

            if (valuesAdded <= 0)
                continue;

            appSettings[section] = sectionNode;
            sectionsUpdated++;
        }

        return sectionsUpdated > 0;
    }

    private static string CreateSchema(IEnumerable<IGrouping<string, ConfigurationValue>> valuesBySection)
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

    private static void AddSchemaProperties(IEnumerable<ConfigurationValue> values, JsonObject sectionNode)
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

    private static string CreateMarkdownDocumentation(IEnumerable<IGrouping<string, ConfigurationValue>> valuesBySection)
    {
        var text = new StringBuilder()
            .AppendLine("# Configuration Settings");

        static void AddPropertyDocumentation(IEnumerable<ConfigurationValue> values, StringBuilder text)
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

        foreach (var sectionValues in valuesBySection)
        {
            var section = sectionValues.Key;
            var values = sectionValues.ToArray();

            if (values.Length == 0)
                continue;

            text.AppendLine($"## {section}");

            AddPropertyDocumentation(values, text);
        }

        return text.ToString();
    }

    private static string CreateHtmlDocumentation(IEnumerable<IGrouping<string, ConfigurationValue>> valuesBySection)
    {
        const string header = @"<!DOCTYPE html>
<html lang=""en"">
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
";

        var text = new StringBuilder(header)
            .AppendLine("<h2>Configuration Settings</h2>");

        static void AddPropertyDocumentation(IEnumerable<ConfigurationValue> configurationValues, StringBuilder text)
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

        foreach (var valueSection in valuesBySection)
        {
            var section = valueSection.Key;
            var values = valueSection.ToArray();

            if (values.Length == 0)
                continue;

            text.AppendLine($"<h3>{HtmlEncode(section)}</h3>");

            AddPropertyDocumentation(values, text);
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

    private static bool IsSecret(this ConfigurationValue value)
    {
        return value.Property.GetCustomAttribute<SettingsSecretAttribute>() != null;
    }

    private static bool IsIgnored(this ConfigurationValue value)
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
