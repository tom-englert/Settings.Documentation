using TomsToolbox.SampleWebApplication;

var builder = AppBuilder.CreateBuilder(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.RegisterWeatherForecastApi();
app.Run();
