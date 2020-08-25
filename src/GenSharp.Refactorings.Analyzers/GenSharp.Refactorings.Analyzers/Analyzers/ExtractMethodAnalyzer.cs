using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace GenSharp.Refactorings.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExtractMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = DiagnosticIdentifiers.ExtractMethod;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.ExtractMethod);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(actionContext =>
                {
                    actionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExtractMethod, actionContext.Node.GetLocation()));
                },
                SyntaxKind.MethodDeclaration);
        }
    }
}