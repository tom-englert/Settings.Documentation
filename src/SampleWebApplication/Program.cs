using TomsToolbox.Configuration.Documentation;
using TomsToolbox.SampleWebApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddOptions<MyOptions>()
    .BindConfiguration(MyOptions.ConfigurationSection);

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .SettingsDocumentationBuilder(options =>
        {
            options.TargetDirectory = @"D:\dev\GitHub\Configuration.Documentation\src\SampleWebApplication\";
        })
        .GenerateDocumentation();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.RegisterWeatherForecastApi();
app.Run();
