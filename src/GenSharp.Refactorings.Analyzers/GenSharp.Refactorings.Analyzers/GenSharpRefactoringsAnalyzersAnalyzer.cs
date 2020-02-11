using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GenSharpRefactoringsAnalyzersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GenSharp";

        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string _category = "Extract";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Info, true, _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

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

                var diagnostic = Diagnostic.Create(_rule, actionContext.Node.GetLocation(), declaration.Variables.First().Initializer.Value);
                actionContext.ReportDiagnostic(diagnostic);
            }, SyntaxKind.LocalDeclarationStatement);
        }
    }
}
