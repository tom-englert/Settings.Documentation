using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace TomsToolbox.Settings.Documentation.Analyzer;

public static class CodeFixProviderHelper
{
    public static async Task<Document> AddAttributeAsync(this Document document, MemberDeclarationSyntax memberDeclaration, string attributeText, string attributeNamespace, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root is null)
            return document;

        var newAttributeList = (await SyntaxFactory.ParseSyntaxTree($"[{attributeText}]").GetRootAsync(cancellationToken))
            .DescendantNodes()
            .OfType<AttributeListSyntax>()
            .First();

        switch (memberDeclaration)
        {
            case PropertyDeclarationSyntax:
                newAttributeList = newAttributeList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                break;
            case TypeDeclarationSyntax or MethodDeclarationSyntax when memberDeclaration.AttributeLists.Count == 0:
                newAttributeList = newAttributeList.WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                break;
        }

        var updatedMemberDeclaration = memberDeclaration
            .AddAttributeLists(newAttributeList)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root
            .ReplaceNode(memberDeclaration, updatedMemberDeclaration)
            .EnsureUsing(attributeNamespace);

        var newDocument = document.WithSyntaxRoot(newRoot);

        return newDocument;
    }

    public static SyntaxNode EnsureUsing(this SyntaxNode newRoot, string attributeNamespace)
    {
        if (newRoot is not CompilationUnitSyntax compilationUnit)
            return newRoot;

        var hasUsingDirective = compilationUnit.Usings
            .Any(u => string.Equals(u.Name?.ToString(), attributeNamespace, StringComparison.Ordinal));

        if (hasUsingDirective)
            return newRoot;

        var newUsingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(attributeNamespace))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var usings = compilationUnit.Usings
            .Add(newUsingDirective);

        newRoot = compilationUnit.WithUsings(usings);

        return newRoot;
    }
}
