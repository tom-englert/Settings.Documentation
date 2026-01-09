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

            var syntaxNode = root?.FindNode(diagnosticSpan);

            // Handle diagnostic on invocation site (e.g., AddOptions<T>() inside a generic method)
            if (syntaxNode is not GenericNameSyntax genericName)
                continue;

            // Find the containing method
            var containingMethod = genericName.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (containingMethod == null)
                continue;

            var methodName = containingMethod.Identifier.Text;

            var codeAction = CodeAction.Create(
                $"Add [SettingsAddOptionsInvocator] attribute to {methodName}",
                cancellationToken => context.Document.AddAttributeAsync(containingMethod, "SettingsAddOptionsInvocator", "TomsToolbox.Settings.Documentation.Abstractions", cancellationToken),
                "AddSettingsAddOptionsInvocatorAttributeToMethod");

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
