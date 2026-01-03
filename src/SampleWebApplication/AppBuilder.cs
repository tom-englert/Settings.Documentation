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

        services.AddOptions<DatabaseConnectionStrings>()
            .BindConfiguration(DatabaseConnectionStrings.ConfigurationSection);
        
        services.AddOptions<MessageQueueConnectionStrings>()
            .BindConfiguration(MessageQueueConnectionStrings.ConfigurationSection);

        return builder;
    }
}
