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

        // Extract comment trivia from the member declaration
        var leadingTrivia = memberDeclaration.GetLeadingTrivia();
        var commentTrivia = ExtractCommentTriviaWithLeadingWhitespace(leadingTrivia).ToArray();
        var remainingTrivia = leadingTrivia.Skip(commentTrivia.Length).ToArray();

        // Prepare the updated member declaration
        MemberDeclarationSyntax updatedMemberDeclaration;

        // If there are comments, move them before the attribute
        if (commentTrivia.Any(IsCommentTrivia))
        {
            // When comments exist, don't add extra newlines to the attribute
            switch (memberDeclaration)
            {
                case PropertyDeclarationSyntax:
                    newAttributeList = newAttributeList.WithTrailingTrivia(NewLine());
                    break;
            }

            // Keep only the final indentation (whitespace before the member itself)
            var finalIndentation = remainingTrivia.LastOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
            var newLeadingTrivia = finalIndentation.IsKind(SyntaxKind.WhitespaceTrivia) 
                ? SyntaxFactory.TriviaList(finalIndentation) 
                : SyntaxFactory.TriviaList();

            updatedMemberDeclaration = memberDeclaration
                .WithLeadingTrivia(newLeadingTrivia)
                .AddAttributeLists(newAttributeList)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Add comment trivia before the updated member (which now has the attribute)
            updatedMemberDeclaration = updatedMemberDeclaration
                .WithLeadingTrivia(commentTrivia.Concat(updatedMemberDeclaration.GetLeadingTrivia()));
        }
        else
        {
            switch (memberDeclaration)
            {
                case PropertyDeclarationSyntax:
                    newAttributeList = newAttributeList.WithTrailingTrivia(NewLine());
                    break;
                case TypeDeclarationSyntax or MethodDeclarationSyntax when memberDeclaration.AttributeLists.Count == 0:
                    newAttributeList = newAttributeList.WithLeadingTrivia(NewLine());
                    break;
            }

            updatedMemberDeclaration = memberDeclaration
                .AddAttributeLists(newAttributeList)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

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
            .WithTrailingTrivia(NewLine())
            .WithAdditionalAnnotations(Formatter.Annotation);

        var usings = compilationUnit.Usings
            .Add(newUsingDirective);

        newRoot = compilationUnit.WithUsings(usings);

        return newRoot;
    }

        private static IEnumerable<SyntaxTrivia> ExtractCommentTriviaWithLeadingWhitespace(SyntaxTriviaList triviaList)
        {
            // Find the first comment trivia
            var firstCommentIndex = -1;
            for (int i = 0; i < triviaList.Count; i++)
            {
                if (IsCommentTrivia(triviaList[i]))
                {
                    firstCommentIndex = i;
                    break;
                }
            }

            if (firstCommentIndex == -1)
                return [];

            // Include everything from the startup to and including all comment-related trivia
            var result = new List<SyntaxTrivia>();
            for (int i = 0; i < triviaList.Count; i++)
            {
                var trivia = triviaList[i];
                if (i < firstCommentIndex || IsCommentTrivia(trivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    result.Add(trivia);
                }
                else if (i > firstCommentIndex && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    // This is indentation between comment lines or after comments
                    // Check if there's more comment trivia after this
                    bool hasMoreComments = false;
                    for (int j = i + 1; j < triviaList.Count; j++)
                    {
                        if (IsCommentTrivia(triviaList[j]))
                        {
                            hasMoreComments = true;
                            break;
                        }
                        if (!triviaList[j].IsKind(SyntaxKind.EndOfLineTrivia))
                            break;
                    }

                    if (hasMoreComments)
                        result.Add(trivia);
                    else
                        break; // This is the final indentation before the member
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia);
        }

        private static SyntaxTrivia NewLine()
        {
            return SyntaxFactory.EndOfLine(Environment.NewLine);
        }
    }
