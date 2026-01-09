using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TomsToolbox.Settings.Documentation.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SettingsDocumentationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
            Diagnostics.MissingDescriptionAttribute,
            Diagnostics.MissingSettingsSectionAttribute,
            Diagnostics.MissingInvocatorAttribute
        ]);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSymbolAction(AnalyzeNamedTypeWithSettingsSectionAttribute, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedTypeWithSettingsSectionAttribute(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Only analyze types that have the SettingsSection attribute
            var hasSettingsSectionAttribute = namedTypeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsSectionAttribute" or "SettingsSection");

            if (!hasSettingsSectionAttribute)
                return;

            // Check if type has an ignore attribute
            var hasSettingsIgnoreAttribute = namedTypeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsIgnoreAttribute" or "SettingsIgnore");

            if (hasSettingsIgnoreAttribute)
                return;

            // Analyze properties for missing descriptions
            AnalyzeConfigurationPropertiesForSymbol(context, namedTypeSymbol);
        }

        private static void AnalyzeConfigurationPropertiesForSymbol(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
        {
            var properties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsReadOnly && !p.IsStatic);

            foreach (var property in properties)
            {
                var attributes = property.GetAttributes();

                var hasSettingsIgnoreAttribute = attributes.Any(attr =>
                    attr.AttributeClass?.Name is "SettingsIgnoreAttribute" or "SettingsIgnore");

                if (hasSettingsIgnoreAttribute)
                    continue;

                var hasDescriptionAttribute = attributes.Any(attr =>
                    attr.AttributeClass?.Name is "DescriptionAttribute" or "Description");

                if (hasDescriptionAttribute)
                    continue;

                var propertyLocation = property.Locations.FirstOrDefault();
                if (propertyLocation is null || !propertyLocation.IsInSource)
                    continue;

                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingDescriptionAttribute,
                    propertyLocation,
                    property.Name, typeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check if this is a member access expression (e.g., services.AddOptions<T>())
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            // Check if the method name is "AddOptions"
            if (memberAccess.Name is not GenericNameSyntax genericName)
                return;

            // Get the symbol info for the invocation
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            if (!string.Equals(genericName.Identifier.Text, "AddOptions", StringComparison.Ordinal))
            {
                if (!methodSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name.StartsWith("SettingsAddOptionsInvocator", StringComparison.Ordinal) == true))
                    return;
            }
            else
            {
                if (methodSymbol.ContainingType?.ToDisplayString() != "Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions")
                    return;
            }

            // Get the type argument T from AddOptions<T>()
            if (methodSymbol.TypeArguments.Length != 1)
                return;

            var typeArgument = methodSymbol.TypeArguments[0];

            if (typeArgument is ITypeParameterSymbol)
            {
                var containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();

                if (containingMethod == null || containingMethod.AttributeLists.SelectMany(list => list.Attributes).Any(attr => attr.Name.ToString().StartsWith("SettingsAddOptionsInvocator")))
                    return;

                var methodIdentifier = containingMethod.Identifier;

                // Report diagnostic at the invocation site (genericName) not at the method declaration
                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingInvocatorAttribute,
                    genericName.GetLocation(),
                    methodIdentifier);

                context.ReportDiagnostic(diagnostic);

                return;
            }

            // Ensure we have a valid named type symbol
            if (typeArgument is not INamedTypeSymbol namedTypeSymbol)
                return;

            var hasSettingsIgnoreAttribute = namedTypeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsIgnoreAttribute" or "SettingsIgnore");

            if (hasSettingsIgnoreAttribute)
                return;

            var hasSettingsSectionAttribute = namedTypeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsSectionAttribute" or "SettingsSection");

            // Only report diagnostics if the type is defined in source code within this compilation
            var typeLocation = namedTypeSymbol.Locations.FirstOrDefault();
            if (typeLocation is null || !typeLocation.IsInSource)
                return;

            // Report missing [SettingsSection] attribute at the invocation site
            if (!hasSettingsSectionAttribute)
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingSettingsSectionAttribute,
                    genericName.GetLocation(),
                    namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }

            // Note: Property diagnostics are handled by AnalyzeNamedTypeWithSettingsSectionAttribute
            // when the type has [SettingsSection] attribute
        }
    }
}
