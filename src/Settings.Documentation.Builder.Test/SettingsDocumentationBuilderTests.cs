#nullable disable

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using TomsToolbox.Settings.Documentation.Abstractions;
// ReSharper disable AssignNullToNotNullAttribute

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace TomsToolbox.Settings.Documentation.Builder.Test;

[TestClass]
public class SettingsDocumentationBuilderTests
{
    [TestMethod]
    public void WhenBuildingFromDictionary_ShouldDeserializeCorrectly()
    {
        var configurationData = new Dictionary<string, string>
        {
            ["TestOptions:Port"] = "8080",
            ["TestOptions:Host"] = "example.com",
            ["TestOptions:IsEnabled"] = "true",
            ["TestOptions:Timeout"] = "00:05:00",
            ["TestOptions:MaxRetries"] = "5"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var testOptions = new TestOptions();
        configuration.GetSection("TestOptions").Bind(testOptions);

        Assert.AreEqual(8080, testOptions.Port);
        Assert.AreEqual("example.com", testOptions.Host);
        Assert.IsTrue(testOptions.IsEnabled);
        Assert.AreEqual(TimeSpan.FromMinutes(5), testOptions.Timeout);
        Assert.AreEqual(5, testOptions.MaxRetries);
    }

    [TestMethod]
    public void WhenBuildingFromDictionary_WithMissingValues_ShouldUseDefaults()
    {
        var configurationData = new Dictionary<string, string>
        {
            ["TestOptions:Port"] = "8080"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var testOptions = new TestOptions();
        configuration.GetSection("TestOptions").Bind(testOptions);

        Assert.AreEqual(8080, testOptions.Port);
        Assert.AreEqual("localhost", testOptions.Host);
        Assert.IsFalse(testOptions.IsEnabled);
        Assert.AreEqual(TimeSpan.FromSeconds(30), testOptions.Timeout);
        Assert.AreEqual(3, testOptions.MaxRetries);
    }

    [TestMethod]
    public void WhenBuildingFromDictionary_WithCollections_ShouldDeserializeCorrectly()
    {
        var configurationData = new Dictionary<string, string>
        {
            ["TestOptions:Tags:0"] = "tag1",
            ["TestOptions:Tags:1"] = "tag2",
            ["TestOptions:Tags:2"] = "tag3"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var testOptions = new TestOptions();
        configuration.GetSection("TestOptions").Bind(testOptions);

        Assert.IsNotNull(testOptions.Tags);
        Assert.HasCount(3, testOptions.Tags);
        CollectionAssert.AreEqual(new[] { "tag1", "tag2", "tag3" }, testOptions.Tags.ToArray());
    }

    [TestMethod]
    public void WhenObjectIsDecoratedWithDataContract_ShouldBindAllPublicPropertiesAndIgnoreDataMemberAnnotations()
    {
        var configurationData = new Dictionary<string, string>
        {
            ["DataContractOptions:ApiKey"] = "secret-key-123",
            ["DataContractOptions:Endpoint"] = "https://api.example.com",
            ["DataContractOptions:InternalProperty"] = "internal-value"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var options = new DataContractOptions();
        configuration.GetSection("DataContractOptions").Bind(options);

        Assert.AreEqual("secret-key-123", options.ApiKey);
        Assert.AreEqual("https://api.example.com", options.Endpoint);
        Assert.AreEqual("internal-value", options.InternalPropertyRenamed);
        Assert.AreEqual("internal-value", options.InternalProperty);
    }

    [TestMethod]
    public void WhenCreatingSettingsValue_WithDataMemberName_ShouldUsePropertyName()
    {
        var defaultInstance = new DataContractOptions();
        var propertyInfo = typeof(DataContractOptions).GetProperty(nameof(DataContractOptions.ApiKey));

        var settingsValue = SettingsValue.Create("DataContractOptions", propertyInfo, defaultInstance);

        Assert.AreEqual("ApiKey", settingsValue.Name);
        Assert.AreEqual("DataContractOptions", settingsValue.Section);
        Assert.AreEqual("The API key for authentication", settingsValue.Description);
    }

    [TestMethod]
    public void WhenCreatingSettingsValue_FromDataContract_ShouldExtractMetadata()
    {
        var types = new[] { typeof(DataContractOptions) };
        
        var context = SettingsDocumentation.SettingsDocumentationBuilder(types);

        var values = context.Values.ToList();
        
        Assert.HasCount(3 , values, "Should have 3 documented properties");
        
        var apiKeyValue = values.FirstOrDefault(v => v.Name == "ApiKey");
        Assert.IsNotNull(apiKeyValue);
        Assert.AreEqual("The API key for authentication", apiKeyValue.Description);
        Assert.IsTrue(apiKeyValue.IsSecret);
        
        var endpointValue = values.FirstOrDefault(v => v.Name == "Endpoint");
        Assert.IsNotNull(endpointValue);
        Assert.AreEqual("The service endpoint URL", endpointValue.Description);
        Assert.IsFalse(endpointValue.IsSecret);

        var internalValue = values.FirstOrDefault(v => v.Name == "InternalProperty");
        Assert.IsNotNull(internalValue);
        Assert.AreEqual("A Description", internalValue.Description);
        Assert.IsFalse(internalValue.IsSecret);
    }

    [TestMethod]
    public void WhenCreatingSettingsValue_WithRequiredAttribute_ShouldMarkAsRequired()
    {
        var types = new[] { typeof(DataContractOptions) };
        
        var context = SettingsDocumentation.SettingsDocumentationBuilder(types);

        var values = context.Values.ToList();
        var apiKeyValue = values.FirstOrDefault(v => v.Name == "ApiKey");
        
        Assert.IsNotNull(apiKeyValue);
        Assert.IsTrue(apiKeyValue.IsRequired);
    }
}

[SettingsSection("TestOptions")]
public class TestOptions
{
    [System.ComponentModel.Description("The port number")]
    public int Port { get; set; } = 8080;

    [System.ComponentModel.Description("The host address")]
    public string Host { get; set; } = "localhost";

    [System.ComponentModel.Description("Whether the service is enabled")]
    public bool IsEnabled { get; set; } = false;

    [System.ComponentModel.Description("Request timeout duration")]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    [System.ComponentModel.Description("Maximum number of retry attempts")]
    public int MaxRetries { get; set; } = 3;

    [System.ComponentModel.Description("Collection of tags")]
    public IReadOnlyCollection<string> Tags { get; set; } = [];
}

/// <summary>
/// Sample settings class decorated with DataContract and DataMember attributes, to verify these have no effect on configuration binding.
/// </summary>
[DataContract]
[SettingsSection("DataContractOptions")]
public class DataContractOptions
{
    [DataMember]
    [System.ComponentModel.Description("The API key for authentication")]
    [SettingsSecret]
    [Required]
    public string ApiKey { get; set; }

    // The DataMember attribute renames the property in serialization, but the configuration binder uses the property name
    [DataMember(Name = "PublicEndpoint")]
    [System.ComponentModel.Description("The service endpoint URL")]
    public string Endpoint { get; set; }

    // This property is not marked with [DataMember], but should still be bound by the configuration binder
    // Also [SettingsBindable(false)] has no effect here
    [SettingsBindable(false)]
    [ConfigurationKeyName("InternalProperty")]
    [System.ComponentModel.Description("A Description")]
    public string InternalPropertyRenamed { get; set; }

    public string InternalProperty => InternalPropertyRenamed;
}
