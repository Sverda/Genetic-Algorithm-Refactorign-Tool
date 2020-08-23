using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class AnalyzerResult
    {
        private readonly IList<ITypeParameterSymbol> _typeParametersInDeclaration;
        private readonly IList<ITypeParameterSymbol> _typeParametersInConstraintList;
        private readonly IList<VariableInfo> _variables;
        private readonly VariableInfo _variableToUseAsReturnValue;

        public AnalyzerResult(
            SemanticDocument document,
            IEnumerable<ITypeParameterSymbol> typeParametersInDeclaration,
            IEnumerable<ITypeParameterSymbol> typeParametersInConstraintList,
            IList<VariableInfo> variables,
            VariableInfo variableToUseAsReturnValue,
            ITypeSymbol returnType,
            bool awaitTaskReturn,
            bool instanceMemberIsUsed,
            bool shouldBeReadOnly,
            bool endOfSelectionReachable)
        {
            UseInstanceMember = instanceMemberIsUsed;
            ShouldBeReadOnly = shouldBeReadOnly;
            EndOfSelectionReachable = endOfSelectionReachable;
            AwaitTaskReturn = awaitTaskReturn;
            SemanticDocument = document;
            _typeParametersInDeclaration = typeParametersInDeclaration.ToList();
            _typeParametersInConstraintList = typeParametersInConstraintList.ToList();
            _variables = variables;
            ReturnType = returnType;
            _variableToUseAsReturnValue = variableToUseAsReturnValue;
        }

        public bool UseInstanceMember { get; }

        public bool ShouldBeReadOnly { get; }

        public bool EndOfSelectionReachable { get; }

        public SemanticDocument SemanticDocument { get; }

        public bool AwaitTaskReturn { get; }

        public ITypeSymbol ReturnType { get; }

        public bool HasVariableToUseAsReturnValue => _variableToUseAsReturnValue != null;

        public VariableInfo VariableToUseAsReturnValue => _variableToUseAsReturnValue;

        public bool HasReturnType => ReturnType.SpecialType != SpecialType.System_Void && !AwaitTaskReturn;

        public IEnumerable<VariableInfo> MethodParameters => _variables.Where(v => v.UseAsParameter);

        public IEnumerable<VariableInfo> GetVariablesToSplitOrMoveIntoMethodDefinition(CancellationToken cancellationToken)
        {
            return _variables
                .Where(v => v.GetDeclarationBehavior(cancellationToken) == DeclarationBehavior.SplitIn ||
                            v.GetDeclarationBehavior(cancellationToken) == DeclarationBehavior.MoveIn);
        }
    }
}
