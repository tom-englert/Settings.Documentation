using TomsToolbox.Settings.Documentation;

var solutionDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
while (solutionDir.Parent != null && !solutionDir.GetFiles("*.sln*").Any())
{
    solutionDir = solutionDir.Parent;
}

var builder = TomsToolbox.SampleWebApplication.AppBuilder.CreateBuilder(args);

builder.Services
    .SettingsDocumentationBuilder(options =>
    {
        options.TargetDirectory = Path.Combine(solutionDir.FullName, "SampleWebApplication");
    })
    .GenerateDocumentation();


