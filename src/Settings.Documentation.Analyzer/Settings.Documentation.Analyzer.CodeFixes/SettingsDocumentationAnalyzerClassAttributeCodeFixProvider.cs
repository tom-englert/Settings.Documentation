using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomsToolbox.Settings.Documentation.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SettingsDocumentationAnalyzerClassAttributeCodeFixProvider)), Shared]
public class SettingsDocumentationAnalyzerClassAttributeCodeFixProvider : CodeFixProvider
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

            // Handle diagnostic on invocation site (e.g., AddOptions<MyType>)
            if (syntaxNode is not GenericNameSyntax genericName)
                continue;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                continue;

            // Get the type argument from the generic name
            if (genericName.TypeArgumentList.Arguments.Count != 1)
                continue;

            var typeArgumentSyntax = genericName.TypeArgumentList.Arguments[0];
            var typeSymbol = semanticModel.GetSymbolInfo(typeArgumentSyntax, context.CancellationToken).Symbol as INamedTypeSymbol;
            if (typeSymbol == null)
                continue;

            // Find the document containing the type declaration
            var typeLocation = typeSymbol.Locations.FirstOrDefault(loc => loc.IsInSource);
            if (typeLocation == null)
                continue;

            var typeDocument = context.Document.Project.GetDocument(typeLocation.SourceTree);
            if (typeDocument == null)
                continue;

            var typeRoot = await typeDocument.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var typeDeclarationNode = typeRoot?.FindNode(typeLocation.SourceSpan) as TypeDeclarationSyntax;

            if (typeDeclarationNode == null)
                continue;

            var codeAction = CodeAction.Create(
                $"Add [SettingsSection] attribute to {typeSymbol.Name}",
                cancellationToken => typeDocument.AddAttributeAsync(typeDeclarationNode, "SettingsSection", "TomsToolbox.Settings.Documentation.Abstractions", cancellationToken),
                "AddSettingsSectionAttributeToType");

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
