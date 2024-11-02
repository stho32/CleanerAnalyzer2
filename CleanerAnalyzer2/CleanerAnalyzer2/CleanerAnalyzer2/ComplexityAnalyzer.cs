using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CleanerAnalyzer2
{
    /// <summary>
    /// Analyzes methods for cyclomatic complexity.
    /// </summary>
    /// <remarks>
    /// This analyzer checks if methods exceed a certain threshold of cyclomatic complexity.
    /// High complexity can indicate that a method is difficult to understand and maintain.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComplexityAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "CyclomaticComplexity";
        private const string Title = "Method has high cyclomatic complexity";
        private const string MessageFormat = "Method '{0}' has a cyclomatic complexity of {1}, which exceeds the threshold of {2}";
        private const string Description = "Methods should have low cyclomatic complexity for better maintainability.";
        private const string Category = "Maintainability";

        private const int ComplexityThreshold = 4;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            
            // Check if the method should be included in the complexity analysis
            if (!ShouldAnalyzeMethod(methodDeclaration.Identifier.Text))
            {
                return;
            }

            int complexity = CalculateCyclomaticComplexity(methodDeclaration);

            if (complexity > ComplexityThreshold)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), 
                    methodDeclaration.Identifier.Text, complexity, ComplexityThreshold);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool ShouldAnalyzeMethod(string methodName)
        {
            // Exclude methods starting with "Pruefe" and ending with "MitRegeln"
            if (methodName.StartsWith("Pruefe") && methodName.EndsWith("MitRegeln"))
            {
                return false;
            }

            // Add any other exclusion rules here if needed

            return true;
        }

        private int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
        {
            var decisionPoints = method.DescendantNodes().OfType<SyntaxNode>()
                .Count(node => node is IfStatementSyntax
                               || node is WhileStatementSyntax
                               || node is ForStatementSyntax
                               || node is ForEachStatementSyntax
                               || node is CaseSwitchLabelSyntax
                               || node is DefaultSwitchLabelSyntax
                               || node is CatchClauseSyntax
                               || node is ConditionalExpressionSyntax);

            return decisionPoints + 1;
        }
    }
}
