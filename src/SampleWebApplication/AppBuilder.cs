using TomsToolbox.Settings.Documentation.Abstractions;
namespace TomsToolbox.SampleWebApplication;

public static class AppBuilder
{
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        services.AddOpenApi()
            .AddOptions<MyOptions>()
            .BindConfiguration(MyOptions.ConfigurationSection);

        services.BindOptions<MyOptions>();

        services.AddOptions<DatabaseConnectionStrings>()
            .BindConfiguration(DatabaseConnectionStrings.ConfigurationSection);

        services.AddOptions<MessageQueueConnectionStrings>()
            .BindConfiguration(MessageQueueConnectionStrings.ConfigurationSection);

        return builder;
    }

    [SettingsAddOptionsInvocator]
    public static IServiceCollection BindOptions<TOptions>(this IServiceCollection services, string? sectionName = null) where TOptions : class, new()
    {
        sectionName ??= typeof(TOptions).Name;

        services
            .AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
