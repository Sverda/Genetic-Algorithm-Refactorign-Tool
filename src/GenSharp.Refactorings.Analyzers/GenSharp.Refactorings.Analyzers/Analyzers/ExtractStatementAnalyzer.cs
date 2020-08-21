using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GenSharp.Refactorings.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExtractStatementAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = DiagnosticIdentifiers.ExtractStatement;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.ExtractStatement);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(actionContext =>
            {
                var declaration = actionContext.Node.DescendantNodes().OfType<VariableDeclarationSyntax>().First();
                var afterEqualitySign = declaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value;
                if (afterEqualitySign is LiteralExpressionSyntax)
                {
                    return;
                }

                foreach (var variable in declaration.Variables)
                {
                    actionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExtractStatement, variable.Identifier.GetLocation()));
                }
            },
            SyntaxKind.LocalDeclarationStatement);
        }
    }
}
