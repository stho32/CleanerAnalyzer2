using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanerAnalyzer2
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RepositoriesShouldBeInternalCodeFixProvider)), Shared]
    public class RepositoriesShouldBeInternalCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(RepositoriesShouldBeInternalAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make repository internal",
                    createChangedDocument: c => MakeInternalAsync(context.Document, declaration, c),
                    equivalenceKey: "MakeRepositoryInternal"),
                diagnostic);
        }

        private async Task<Document> MakeInternalAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Create a new internal modifier with proper spacing
            var internalModifier = SyntaxFactory.Token(SyntaxKind.InternalKeyword)
                .WithLeadingTrivia(SyntaxFactory.Space);

            // Get all modifiers that are NOT access modifiers (if any exist)
            var nonAccessModifiers = typeDecl.Modifiers.Where(m => !IsAccessModifier(m));
            
            // Create new modifier list starting with 'internal'
            var newModifiers = new SyntaxTokenList();
            newModifiers = newModifiers.Add(internalModifier);

            // Add any remaining non-access modifiers
            foreach (var modifier in nonAccessModifiers)
            {
                newModifiers = newModifiers.Add(modifier);
            }

            // Create the new type declaration with updated modifiers
            var newTypeDecl = typeDecl.WithModifiers(newModifiers);

            // Get the new root and create a new document
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);
            return document.WithSyntaxRoot(newRoot);
        }

        private bool IsAccessModifier(SyntaxToken modifier)
        {
            return modifier.IsKind(SyntaxKind.PublicKeyword) ||
                   modifier.IsKind(SyntaxKind.PrivateKeyword) ||
                   modifier.IsKind(SyntaxKind.ProtectedKeyword) ||
                   modifier.IsKind(SyntaxKind.InternalKeyword);
        }
    }
}
