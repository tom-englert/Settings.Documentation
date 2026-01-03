using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomsToolbox.Settings.Documentation.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SettingsDocumentationAnalyzerMethodAttributeCodeFixProvider)), Shared]
public class SettingsDocumentationAnalyzerMethodAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.MissingInvocatorAttribute.Id);

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
            
            // Find the method declaration identified by the diagnostic.
            var syntaxNode = root?.FindNode(diagnosticSpan);
            if (syntaxNode is not MethodDeclarationSyntax methodDeclaration)
                continue;

            // Register a code action that will invoke the fix.
            var codeAction = CodeAction.Create("Add [SettingsAddOptionsInvocator] attribute", ApplyFix, "AddSettingsAddOptionsInvocatorAttribute");

            context.RegisterCodeFix(codeAction, diagnostic);
            continue;

            Task<Document> ApplyFix(CancellationToken c)
            {
                return context.Document.AddAttributeAsync(methodDeclaration, "SettingsAddOptionsInvocator", "TomsToolbox.Settings.Documentation.Abstractions", c);
            }
        }
    }
}
