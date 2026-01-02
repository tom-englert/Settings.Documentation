using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomsToolbox.Configuration.Documentation.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigurationDocumentationAnalyzerClassAttributeCodeFixProvider)), Shared]
public class ConfigurationDocumentationAnalyzerClassAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.MissingSettingsSectionAttribute.Id);

    public sealed override FixAllProvider GetFixAllProvider()
    {

        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var syntaxNode = root?.FindNode(diagnosticSpan);
            if (syntaxNode is not TypeDeclarationSyntax typeDeclaration)
                continue;

            var codeAction = CodeAction.Create("Add [SettingsSection] attribute", ApplyFix, "AddSettingsSectionAttribute");

            context.RegisterCodeFix(codeAction, diagnostic);
            continue;

            Task<Document> ApplyFix(CancellationToken c)
            {
                return context.Document.AddAttributeAsync(typeDeclaration, "SettingsSection", "TomsToolbox.Configuration.Documentation.Abstractions", c);
            }
        }
    }
}
