using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TomsToolbox.Configuration.Documentation.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigurationDocumentationAnalyzer : DiagnosticAnalyzer
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
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
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

            if (genericName.Identifier.Text != "AddOptions")
            {
                if (!methodSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name.StartsWith("SettingsAddOptionsInvocator") == true))
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

                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingInvocatorAttribute,
                    methodIdentifier.GetLocation(),
                    methodIdentifier);

                context.ReportDiagnostic(diagnostic);

                return;
            }

            var hasSettingsIgnoreAttribute = typeArgument.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsIgnoreAttribute" or "SettingsIgnore");

            if (hasSettingsIgnoreAttribute) 
                return;

            var hasSettingsSectionAttribute = typeArgument.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "SettingsSectionAttribute" or "SettingsSection");

            var typeLocation = typeArgument.Locations.FirstOrDefault();
            if (typeLocation is null || !typeLocation.IsInSource)
                return;

            if (!hasSettingsSectionAttribute)
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingSettingsSectionAttribute,
                    typeArgument.Locations[0],
                    typeArgument.Name);

                context.ReportDiagnostic(diagnostic);
            }

            // Now check all writable properties of the type
            AnalyzeConfigurationProperties(context, typeArgument, typeLocation);
        }

        private void AnalyzeConfigurationProperties(SyntaxNodeAnalysisContext context, ITypeSymbol typeSymbol, Location typeLocation)
        {
            var properties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsReadOnly && !p.IsStatic);

            foreach (var property in properties)
            {
                // Check if property has any of the special attributes
                var attributes = property.GetAttributes();

                var hasSettingsIgnoreAttribute = attributes.Any(attr =>
                    attr.AttributeClass?.Name is "SettingsIgnoreAttribute" or "SettingsIgnore");

                if (hasSettingsIgnoreAttribute)
                    continue;

                var hasDescriptionAttribute = attributes.Any(attr =>
                    attr.AttributeClass?.Name is "DescriptionAttribute" or "Description");

                if (hasDescriptionAttribute) 
                    continue;

                var propertyLocation = property.Locations.FirstOrDefault() ?? typeLocation;

                var diagnostic = Diagnostic.Create(
                    Diagnostics.MissingDescriptionAttribute,
                    propertyLocation,
                    property.Name, typeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
