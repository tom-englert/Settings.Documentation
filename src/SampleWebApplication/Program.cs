using System.ComponentModel;
using Microsoft.Extensions.Options;
using TomsToolbox.Configuration.Documentation;
using TomsToolbox.Configuration.Documentation.Abstractions;
using TomsToolbox.SampleWebApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddOptions<MyOptions>()
    .BindConfiguration(nameof(MyOptions));

builder.Services.BuildConfigurationDocumentation();

var app = builder.Build();

var config = app.Services.GetRequiredService<IOptions<MyOptions>>().Value;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.RegisterWeatherForecastApi();
app.Run();

namespace TomsToolbox.SampleWebApplication
{
    [SettingsSection]
    public class MyOptions
    {
        public  const string ConfigurationSection = "Dummy";

        [Description("The port used to connect to the host")]
        public int Port { get; init; } = 99;
        [Description("The host ulr running the service")]
        public string Host { get; init; } = "localhost";
    }
}