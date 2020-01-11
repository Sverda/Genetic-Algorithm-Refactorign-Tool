using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;

namespace GenSharp.Refactorings
{
    public class ExtractDeclarationSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        private readonly GenerateMethodFromStatementSyntaxWalker _methodGenerator;

        public ExtractDeclarationSyntaxRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            _methodGenerator = new GenerateMethodFromStatementSyntaxWalker(_semanticModel);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _methodGenerator.Visit(node.SyntaxTree.GetRoot());
            var members = _methodGenerator.NodeMethodPairs.Select(pair => pair.Method).ToArray();
            var classDeclarationSyntax = node.AddMembers(members);
            return classDeclarationSyntax;
        }

        private SyntaxNode MakeACall(MethodDeclarationSyntax method)
        {
            var className = GetClassIdentifier(method);
            var methodName = SyntaxFactory.IdentifierName(method.Identifier);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, className, methodName);

            //TODO: Parse parameters to arguments
            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("A")));
            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

            var methodCall = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(memberAccess, SyntaxFactory.ArgumentList(argumentList))
                );

            Trace.WriteLine(methodCall.ToFullString());

            return methodCall;
        }

        private static IdentifierNameSyntax GetClassIdentifier(MethodDeclarationSyntax method)
        {
            var ancestors = method.Ancestors().ToList();
            if (!ancestors.Any())
            {
                throw new ArgumentException("No ancestors");
            }

            var parent = ancestors.First();
            var isClassType = parent is ClassDeclarationSyntax;
            if (!isClassType)
            {
                throw new ArgumentException("Direct ancestor is not a class");
            }

            var containingClass = (ClassDeclarationSyntax)parent;
            return SyntaxFactory.IdentifierName(containingClass.Identifier);
        }
    }
}
