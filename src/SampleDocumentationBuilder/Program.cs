using TomsToolbox.Settings.Documentation.Builder;

var solutionDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
while (solutionDir.Parent != null && !solutionDir.EnumerateFiles("*.sln*").Any())
{
    solutionDir = solutionDir.Parent;
}

var targetDirectory = Path.Combine(solutionDir.FullName, "SampleWebApplication");

Console.WriteLine($"Generating settings documentation in {targetDirectory}");


var builder = TomsToolbox.SampleWebApplication.AppBuilder.CreateBuilder(args);

builder.Services
    .SettingsDocumentationBuilder(options =>
    {
        options.TargetDirectory = targetDirectory;
    })
    .GenerateDocumentation();


