using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TomsToolbox.Configuration.Documentation.Analyzer;

namespace Configuration.Documentation.Analyzer;

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
            // Find the type declaration identified by the diagnostic.
            var typeDeclaration = root?.FindNode(diagnosticSpan);
            if (typeDeclaration == null)
                continue;
            // Register a code action that will invoke the fix.
            var codeAction = CodeAction.Create("Add [SettingsSection] attribute", c => CodeFixProviderHelper.AddAttributeAsync(context.Document, (TypeDeclarationSyntax)typeDeclaration, "SettingsSection", "Namespace", c), "AddSettingsSectionAttribute");

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
