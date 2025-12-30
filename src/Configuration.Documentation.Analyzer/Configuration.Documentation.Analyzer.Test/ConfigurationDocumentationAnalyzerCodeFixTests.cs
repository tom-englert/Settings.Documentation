using Configuration.Documentation.Analyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using TomsToolbox.Configuration.Documentation.Abstractions;

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace TomsToolbox.Configuration.Documentation.Analyzer.Test;

[TestClass]
public class ConfigurationDocumentationAnalyzerCodeFixTests
{
    [TestMethod]
    public async Task AddSettingsSectionAttribute_AddsAttributeToClass()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            }
            
            public class {|#0:MyOptions|}
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
                [System.ComponentModel.Description("The host url running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            }
            
            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
                [System.ComponentModel.Description("The host url running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingSettingsSectionAttribute.AsResult().WithArguments("MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddSettingsSectionAttribute_AddsAttributeToClassWithExistingAttributes()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            }
            
            [Serializable]
            public class {|#0:MyOptions|}
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            }
            
            [Serializable]
            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingSettingsSectionAttribute.AsResult().WithArguments("MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    private sealed class Test : CSharpCodeFixTest<ConfigurationDocumentationAnalyzer, ConfigurationDocumentationAnalyzerClassAttributeCodeFixProvider, DefaultVerifier>
    {
        public Test()
        {
            CodeFixTestBehaviors = CodeFixTestBehaviors.SkipLocalDiagnosticCheck;
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([
                new("Microsoft.Extensions.Options.ConfigurationExtensions", "10.0.1")
            ]);

            this.AddReferences(typeof(SettingsSectionAttribute).Assembly);
        }
    }

    public TestContext TestContext { get; set; } = null!;
}
