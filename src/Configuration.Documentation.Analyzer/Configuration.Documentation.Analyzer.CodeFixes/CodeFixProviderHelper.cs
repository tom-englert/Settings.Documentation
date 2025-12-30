
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Configuration.Documentation.Analyzer;

public static class CodeFixProviderHelper
{
    public static async Task<Document> AddAttributeAsync(Document document, MemberDeclarationSyntax memberDeclaration, string attributeText, string attributeNamespace, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root is null)
            return document;

        var newAttributeList = (await SyntaxFactory.ParseSyntaxTree($"[{attributeText}]").GetRootAsync(cancellationToken))
            .DescendantNodes()
            .OfType<AttributeListSyntax>()
            .First();

        var hasExistingAttributes = memberDeclaration.AttributeLists.Count > 0;

        newAttributeList = hasExistingAttributes
            ? newAttributeList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed) // Add trailing line break to keep each attribute on its own line
            : newAttributeList.WithLeadingTrivia(memberDeclaration.GetLeadingTrivia()); // For first attribute, preserve the leading trivia (whitespace/indentation) from the type declaration

        var updatedTypeDeclaration = memberDeclaration
            .AddAttributeLists(newAttributeList)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(memberDeclaration, updatedTypeDeclaration);

        var newDocument = document.WithSyntaxRoot(newRoot);

        return newDocument;
    }
}