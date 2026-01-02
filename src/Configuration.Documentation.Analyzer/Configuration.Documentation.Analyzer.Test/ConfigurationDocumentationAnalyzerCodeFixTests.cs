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

        var test = new ClassAttributeTest
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
    public async Task AddSettingsSectionAttribute_AddsAttributeToClass_PreservesExistingAttributes()
    {
        const string source =
            """
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            
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
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            
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

        var test = new ClassAttributeTest
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
    public async Task AddSettingsSectionAttribute_AddsAttributeToClass_PreservesExistingComments()
    {
        const string source =
            """
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            
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
            
            /// <summary>
            /// Some existing comments
            /// </summary>
            public class {|#0:MyOptions|}
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        const string fixedSource =
            """
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            
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
            /// <summary>
            /// Some existing comments
            /// </summary>
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        var test = new ClassAttributeTest
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
    public async Task AddDescriptionAttribute_AddsAttributeToProperty()
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
            
            [SettingsSection]
            public class MyOptions
            {
                public int {|#0:Port|} { get; init; } = 99;
                [System.ComponentModel.Description("The host url running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System.ComponentModel;
            
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
                [Description("TODO: Add description")]
                public int Port { get; init; } = 99;
                [System.ComponentModel.Description("The host url running the service")]
                public string Host { get; init; } = "localhost";
            }
            """;

        var test = new PropertyAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("Port", "MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddDescriptionAttribute_WhenUsingAlreadyExists_DoesNotDuplicateUsing()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System.ComponentModel;
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
                public string {|#0:ConnectionString|} { get; init; }
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System.ComponentModel;
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
                [Description("TODO: Add description")]
                public string ConnectionString { get; init; }
            }
            """;

        var test = new PropertyAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("ConnectionString", "MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddDescriptionAttribute_ToPropertyWithExistingAttribute_AddsOnNewLine()
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
            
            [SettingsSection]
            public class MyOptions
            {
                [SettingsSecret]
                public string {|#0:ConnectionString|} { get; init; }
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System.ComponentModel;
            
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
                [SettingsSecret]
                [Description("TODO: Add description")]
                public string ConnectionString { get; init; }
            }
            """;

        var test = new PropertyAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("ConnectionString", "MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddDescriptionAttribute_ToPropertyWithXmlComments_PreservesComments()
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
            
            [SettingsSection]
            public class MyOptions
            {
                /// <summary>
                /// The connection string for the database.
                /// </summary>
                public string {|#0:ConnectionString|} { get; init; }
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System.ComponentModel;
            
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
                [Description("TODO: Add description")]
                /// <summary>
                /// The connection string for the database.
                /// </summary>
                public string ConnectionString { get; init; }
            }
            """;

        var test = new PropertyAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("ConnectionString", "MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddDescriptionAttribute_WithComplexPropertyType_AddsCorrectly()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System.Collections.Generic;
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
                public List<string> {|#0:AllowedHosts|} { get; init; }
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using System.Collections.Generic;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System.ComponentModel;

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
                [Description("TODO: Add description")]
                public List<string> AllowedHosts { get; init; }
            }
            """;

        var test = new PropertyAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingDescriptionAttribute.AsResult().WithArguments("AllowedHosts", "MyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddSettingsAddOptionsInvocatorAttribute_AddsAttributeToMethod()
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
                    services.AddMyOptions<MyOptions>();
                }

                static IServiceCollection {|#0:AddMyOptions|}<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
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
                    services.AddMyOptions<MyOptions>();
                }

                [SettingsAddOptionsInvocator]
                static IServiceCollection AddMyOptions<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        var test = new MethodAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingInvocatorAttribute.AsResult().WithArguments("AddMyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddSettingsAddOptionsInvocatorAttribute_AddsAttributeToMethod_PreservesExistingAttributes()
    {
        const string source =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System;

            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
                    services.AddMyOptions<MyOptions>();
                }

                [Obsolete]
                static IServiceCollection {|#0:AddMyOptions|}<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        const string fixedSource =
            """
            using Microsoft.Extensions.DependencyInjection;
            using TomsToolbox.Configuration.Documentation.Abstractions;
            using System;

            static class Application
            {
                static void Program()
                {
                    IServiceCollection services = null!;
                    services.AddMyOptions<MyOptions>();
                }

                [Obsolete]
                [SettingsAddOptionsInvocator]
                static IServiceCollection AddMyOptions<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        var test = new MethodAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingInvocatorAttribute.AsResult().WithArguments("AddMyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddSettingsAddOptionsInvocatorAttribute_AddsAttributeToMethod_PreservesComments()
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
                    services.AddMyOptions<MyOptions>();
                }

                /// <summary>
                /// Adds options configuration.
                /// </summary>
                static IServiceCollection {|#0:AddMyOptions|}<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
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
                    services.AddMyOptions<MyOptions>();
                }

                [SettingsAddOptionsInvocator]
                /// <summary>
                /// Adds options configuration.
                /// </summary>
                static IServiceCollection AddMyOptions<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
            }
            """;

        var test = new MethodAttributeTest
        {
            TestCode = source,
            FixedCode = fixedSource,
            ExpectedDiagnostics =
            {
                Diagnostics.MissingInvocatorAttribute.AsResult().WithArguments("AddMyOptions").WithLocation(0)
            }
        };

        await test.RunAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task AddSettingsAddOptionsInvocatorAttribute_WhenUsingAlreadyExists_DoesNotDuplicateUsing()
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
                    services.AddMyOptions<MyOptions>();
                }

                static IServiceCollection {|#0:AddMyOptions|}<T>(this IServiceCollection services) where T : class
                {
                    services.AddOptions<T>();
                    return services;
                }
            }

            [SettingsSection]
            public class MyOptions
            {
                [System.ComponentModel.Description("The port used to connect to the host")]
                public int Port { get; init; } = 99;
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
                        services.AddMyOptions<MyOptions>();
                    }

                    [SettingsAddOptionsInvocator]
                    static IServiceCollection AddMyOptions<T>(this IServiceCollection services) where T : class
                    {
                        services.AddOptions<T>();
                        return services;
                    }
                }

                [SettingsSection]
                public class MyOptions
                {
                    [System.ComponentModel.Description("The port used to connect to the host")]
                    public int Port { get; init; } = 99;
                }
                """;

            var test = new MethodAttributeTest
            {
                TestCode = source,
                FixedCode = fixedSource,
                ExpectedDiagnostics =
                {
                    Diagnostics.MissingInvocatorAttribute.AsResult().WithArguments("AddMyOptions").WithLocation(0)
                }
            };

            await test.RunAsync(TestContext.CancellationToken);
        }

        private sealed class ClassAttributeTest : CSharpCodeFixTest<ConfigurationDocumentationAnalyzer, ConfigurationDocumentationAnalyzerClassAttributeCodeFixProvider, DefaultVerifier>
    {
        public ClassAttributeTest()
        {
            CodeFixTestBehaviors = CodeFixTestBehaviors.SkipLocalDiagnosticCheck | CodeFixTestBehaviors.FixOne;
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([
                new("Microsoft.Extensions.Options.ConfigurationExtensions", "10.0.1")
            ]);

            this.AddReferences(typeof(SettingsSectionAttribute).Assembly);
        }
    }

        private sealed class PropertyAttributeTest : CSharpCodeFixTest<ConfigurationDocumentationAnalyzer, ConfigurationDocumentationAnalyzerPropertyAttributeCodeFixProvider, DefaultVerifier>
        {
            public PropertyAttributeTest()
            {
                CodeFixTestBehaviors = CodeFixTestBehaviors.SkipLocalDiagnosticCheck | CodeFixTestBehaviors.FixOne;
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([
                    new("Microsoft.Extensions.Options.ConfigurationExtensions", "10.0.1")
                ]);

                this.AddReferences(typeof(SettingsSectionAttribute).Assembly);
            }
        }

        private sealed class MethodAttributeTest : CSharpCodeFixTest<ConfigurationDocumentationAnalyzer, ConfigurationDocumentationAnalyzerMethodAttributeCodeFixProvider, DefaultVerifier>
        {
            public MethodAttributeTest()
            {
                CodeFixTestBehaviors = CodeFixTestBehaviors.SkipLocalDiagnosticCheck | CodeFixTestBehaviors.FixOne;
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([
                    new("Microsoft.Extensions.Options.ConfigurationExtensions", "10.0.1")
                ]);

                this.AddReferences(typeof(SettingsSectionAttribute).Assembly);
            }
        }

        public TestContext TestContext { get; set; } = null!;
    }
