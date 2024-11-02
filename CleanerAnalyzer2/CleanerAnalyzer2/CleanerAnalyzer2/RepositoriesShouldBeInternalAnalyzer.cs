using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace CleanerAnalyzer2
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RepositoriesShouldBeInternalAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RepositoriesShouldBeInternal";

        private static readonly LocalizableString Title = "Repository classes should be internal";
        private static readonly LocalizableString MessageFormat = "Repository class '{0}' should be marked as internal";
        private static readonly LocalizableString Description = "All Repository classes should be internal to prevent direct usage from outside the assembly.";
        private const string Category = "Design";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.TypeKind == TypeKind.Class && 
                namedTypeSymbol.Name.EndsWith("Repository", StringComparison.Ordinal))
            {
                if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Internal)
                {
                    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
