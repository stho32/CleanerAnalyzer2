using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CleanerAnalyzer2
{
    /// <summary>
    /// Analyzes repository classes for usage of DateOnly type.
    /// </summary>
    /// <remarks>
    /// This analyzer checks if repository classes use DateOnly type, which can cause SQL Server compatibility issues.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RepositoryDateOnlyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RepositoryDateOnlyUsage";
        private const string Title = "Repository class should not use DateOnly";
        private const string MessageFormat = "Repository class '{0}' should not use DateOnly {1}";
        private const string Description = "Repository classes should not use DateOnly due to SQL Server compatibility issues.";
        private const string Category = "Design";

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

            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (!classDeclaration.Identifier.Text.EndsWith("Repository"))
                return;

            foreach (var member in classDeclaration.Members)
            {
                if (member is MethodDeclarationSyntax method)
                {
                    AnalyzeMethod(context, method, classDeclaration.Identifier.Text);
                }
                else if (member is FieldDeclarationSyntax field)
                {
                    AnalyzeField(context, field, classDeclaration.Identifier.Text);
                }
            }
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method, string className)
        {
            foreach (var parameter in method.ParameterList.Parameters)
            {
                if (IsDateOnlyType(parameter.Type))
                {
                    var diagnostic = Diagnostic.Create(Rule, parameter.GetLocation(), className, $"parameter '{parameter.Identifier.Text}'");
                    context.ReportDiagnostic(diagnostic);
                }
            }

            var dateOnlyVariables = method.DescendantNodes()
                .OfType<VariableDeclarationSyntax>()
                .Where(v => IsDateOnlyType(v.Type));

            foreach (var variable in dateOnlyVariables)
            {
                var diagnostic = Diagnostic.Create(Rule, variable.GetLocation(), className, $"variable in method '{method.Identifier.Text}'");
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeField(SyntaxNodeAnalysisContext context, FieldDeclarationSyntax field, string className)
        {
            if (IsDateOnlyType(field.Declaration.Type))
            {
                var diagnostic = Diagnostic.Create(Rule, field.GetLocation(), className, $"field '{field.Declaration.Variables.First().Identifier.Text}'");
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsDateOnlyType(TypeSyntax type)
        {
            return type?.ToString() == "DateOnly";
        }
    }
}
