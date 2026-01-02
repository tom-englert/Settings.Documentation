using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomsToolbox.Configuration.Documentation.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigurationDocumentationAnalyzerPropertyAttributeCodeFixProvider)), Shared]
public class ConfigurationDocumentationAnalyzerPropertyAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.MissingDescriptionAttribute.Id);

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
            // Find the property declaration identified by the diagnostic.
            var syntaxNode = root?.FindNode(diagnosticSpan);
            if (syntaxNode is not PropertyDeclarationSyntax propertyDeclaration)
                continue;

            // Register a code action that will invoke the fix.
            var codeAction = CodeAction.Create("Add [Description] attribute", ApplyFix, "AddDescriptionAttribute");

            context.RegisterCodeFix(codeAction, diagnostic);
            continue;

            Task<Document> ApplyFix(CancellationToken c)
            {
                return context.Document.AddAttributeAsync(propertyDeclaration, "Description(\"TODO: Add description\")", "System.ComponentModel", c);
            }
        }

    }
}
