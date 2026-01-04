using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyInjection;
using TomsToolbox.Settings.Documentation.Abstractions;

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace TomsToolbox.Settings.Documentation.Analyzer.Test;

[TestClass]
public class SettingsDocumentationAnalyzerUnitTest
{
    [TestMethod]
    public async Task WhenConfigClassIsConfiguredWithAllAttributes_NoDiagnosticIsEmitted()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Settings.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddSomeService()
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            
                static IServiceCollection AddSomeService(this IServiceCollection services)
                {
                    return services;
                }
            }
            
            [SettingsSection]
            public class {|#0:MyOptions|}
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
                [System.ComponentModel.Description("The host ulr running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task WhenConfigClassIsConfiguredButHasNoSectionAttribute_DiagnosticIsEmitted()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddSomeService()
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            
                static IServiceCollection AddSomeService(this IServiceCollection services)
                {
                    return services;
                }
            }
            
            public class {|#0:MyOptions|}
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
                [System.ComponentModel.Description("The host ulr running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source,
            ExpectedDiagnostics = { Diagnostics.MissingSettingsSectionAttribute.AsResult().WithArguments("MyOptions").WithLocation(0) }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task WhenConfigClassIsConfiguredButHasNoAttributes_DiagnosticIsEmitted()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .AddSomeService()
                        .AddOptions<MyOptions>()
                        .BindConfiguration("MyOptions");
                }
            
                static IServiceCollection AddSomeService(this IServiceCollection services)
                {
                    return services;
                }
            }
            
            public class {|#0:MyOptions|}
            {
                public int {|#1:Port|} { get; init; } = 99;
                [System.ComponentModel.Description("The host ulr running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingSettingsSectionAttribute.AsResult().WithArguments("MyOptions").WithLocation(0),
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("Port", "MyOptions").WithLocation(1)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task WhenConfigClassIsIndirectlyConfiguredButHasNoAttributes_DiagnosticIsEmitted()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .CustomAddOptions<MyOptions>();
                }
            
                static IServiceCollection {|#0:CustomAddOptions|}<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>()
                        .BindConfiguration(typeof(T).Name);

                    return services;
                }
            }
            
            public class MyOptions
            {
                public int {|#1:Port|} { get; init; } = 99;
                [System.ComponentModel.Description("The host ulr running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingInvocatorAttribute.AsResult().WithArguments("CustomAddOptions").WithLocation(0),
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task WhenConfigClassIsIndirectlyConfiguredAndHasInvocatorAttribute_DiagnosticIsEmitted()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Settings.Documentation.Abstractions;
            
            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
            
                    services
                        .CustomAddOptions<MyOptions>();
                }
            
                [SettingsAddOptionsInvocator]
                static IServiceCollection CustomAddOptions<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>()
                        .BindConfiguration(typeof(T).Name);

                    return services;
                }
            }
            
            public class {|#0:MyOptions|}
            {
                public int {|#1:Port|} { get; init; } = 99;
                [System.ComponentModel.Description("The host ulr running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new Test
        {
            TestCode = source,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingSettingsSectionAttribute.AsResult().WithArguments("MyOptions").WithLocation(0),
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("Port", "MyOptions").WithLocation(1)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    private sealed class Test : CSharpAnalyzerTest<SettingsDocumentationAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([
                new("Microsoft.Extensions.Options.ConfigurationExtensions", "10.0.1")
            ]);

            this.AddReferences(typeof(SettingsSectionAttribute).Assembly);
        }
    }

    public TestContext TestContext { get; set; }
}

// Reference code:
#pragma warning disable IDE0051
#nullable disable
// ReSharper disable once ExpressionIsAlwaysNull

internal static class Application
{
    private static void Program()
    {
        IServiceCollection services = null;

        services
            .AddSomeService()
            .AddOptions<MyOptions>()
            .BindConfiguration("MyOptions");
    }

    private static IServiceCollection AddSomeService(this IServiceCollection services)
    {
        return services;
    }
}

/// <summary>
/// Some documentation about MyOptions
/// </summary>
[SettingsSection]
public class MyOptions
{
    [System.ComponentModel.Description("The port used to connect to the host")]
    public int Port { get; init; } = 99;
    [System.ComponentModel.Description("The host ulr running the service")]
    public string Host { get; init; } = "localhost";
}
