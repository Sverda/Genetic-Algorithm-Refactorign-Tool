using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class MethodExtractorAnalyzer
    {
        private readonly SemanticModel _semanticModel;
        private readonly SemanticDocument _semanticDocument;
        private readonly SelectionResult _selectionResult;
        private readonly CancellationToken _cancellationToken;

        private static readonly HashSet<int> _sNonNoisySyntaxKindSet = new HashSet<int>(new int[] { (int)SyntaxKind.WhitespaceTrivia, (int)SyntaxKind.EndOfLineTrivia });

        public MethodExtractorAnalyzer(SemanticDocument semanticDocument,
            SelectionResult selectionResult,
            CancellationToken cancellationToken)
        {
            _semanticDocument = semanticDocument;
            _semanticModel = _semanticDocument.SemanticModel;
            _selectionResult = selectionResult;
            _cancellationToken = cancellationToken;
        }

        public AnalyzerResult Analyze()
        {
            var dataFlowAnalysis = _semanticModel.AnalyzeDataFlow(_selectionResult.FirstStatement(), _selectionResult.LastStatement());
            var symbolMap = GetSymbolMap(_semanticModel);

            GenerateVariableInfoMap(_semanticModel, dataFlowAnalysis, symbolMap, out var variableInfoMap);

            var isInExpressionOrHasReturnStatement = IsInExpressionOrHasReturnStatement(_semanticModel);
            var (parameters, returnType, variableToUseAsReturnValue) =
                GetSignatureInformation(dataFlowAnalysis, variableInfoMap, isInExpressionOrHasReturnStatement);

            var typeParametersInDeclaration = new List<ITypeParameterSymbol>();
            var typeParametersInConstraintList = new List<ITypeParameterSymbol>();
            var awaitTaskReturn = false;
            var instanceMemberIsUsed = false;
            var shouldBeReadOnly = false;
            var endOfSelectionReachable = true;
            return new AnalyzerResult(
                _semanticDocument,
                typeParametersInDeclaration,
                typeParametersInConstraintList,
                parameters,
                variableToUseAsReturnValue,
                returnType,
                awaitTaskReturn,
                instanceMemberIsUsed,
                shouldBeReadOnly,
                endOfSelectionReachable);
        }

        private Dictionary<ISymbol, List<SyntaxToken>> GetSymbolMap(SemanticModel model)
        {
            var context = _selectionResult.GetContainingScope();
            var symbolMap = SymbolMapBuilder.Build(model, context, _selectionResult.GetFinalSpan(), _cancellationToken);
            return symbolMap;
        }

        private void GenerateVariableInfoMap(
                SemanticModel model,
                DataFlowAnalysis dataFlowAnalysisData,
                Dictionary<ISymbol, List<SyntaxToken>> symbolMap,
                out IDictionary<ISymbol, VariableInfo> variableInfoMap)
        {
            variableInfoMap = new Dictionary<ISymbol, VariableInfo>();

            // create map of each data
            var capturedMap = new HashSet<ISymbol>(dataFlowAnalysisData.Captured);
            var dataFlowInMap = new HashSet<ISymbol>(dataFlowAnalysisData.DataFlowsIn);
            var dataFlowOutMap = new HashSet<ISymbol>(dataFlowAnalysisData.DataFlowsOut);
            var alwaysAssignedMap = new HashSet<ISymbol>(dataFlowAnalysisData.AlwaysAssigned);
            var variableDeclaredMap = new HashSet<ISymbol>(dataFlowAnalysisData.VariablesDeclared);
            var readInsideMap = new HashSet<ISymbol>(dataFlowAnalysisData.ReadInside);
            var writtenInsideMap = new HashSet<ISymbol>(dataFlowAnalysisData.WrittenInside);
            var readOutsideMap = new HashSet<ISymbol>(dataFlowAnalysisData.ReadOutside);
            var writtenOutsideMap = new HashSet<ISymbol>(dataFlowAnalysisData.WrittenOutside);
            var unsafeAddressTakenMap = new HashSet<ISymbol>(dataFlowAnalysisData.UnsafeAddressTaken);

            // gather all meaningful symbols for the span.
            var candidates = new HashSet<ISymbol>(readInsideMap);
            candidates.UnionWith(writtenInsideMap);
            candidates.UnionWith(variableDeclaredMap);

            foreach (var symbol in candidates)
            {
                var captured = capturedMap.Contains(symbol);
                var dataFlowIn = dataFlowInMap.Contains(symbol);
                var dataFlowOut = dataFlowOutMap.Contains(symbol);
                var alwaysAssigned = alwaysAssignedMap.Contains(symbol);
                var variableDeclared = variableDeclaredMap.Contains(symbol);
                var readInside = readInsideMap.Contains(symbol);
                var writtenInside = writtenInsideMap.Contains(symbol);
                var readOutside = readOutsideMap.Contains(symbol);
                var writtenOutside = writtenOutsideMap.Contains(symbol);
                var unsafeAddressTaken = unsafeAddressTakenMap.Contains(symbol);

                // make sure readoutside is true when dataflowout is true
                // when a variable is only used inside of loop, a situation where dataflowout == true and readOutside == false
                // can happen. but for extract method's point of view, this is not an information that would affect output.
                // so, here we adjust flags to follow predefined assumption.
                readOutside = readOutside || dataFlowOut;

                // make sure data flow out is true when declared inside/written inside/read outside/not written outside are true
                dataFlowOut = dataFlowOut || (variableDeclared && writtenInside && readOutside && !writtenOutside);

                // variable that is declared inside but never referenced outside. just ignore it and move to next one.
                if (variableDeclared && !dataFlowOut && !readOutside && !writtenOutside)
                {
                    continue;
                }

                // parameter defined inside of the selection (such as lambda parameter) will be ignored
                if (symbol is IParameterSymbol && variableDeclared)
                {
                    continue;
                }

                var type = GetSymbolType(symbol);
                if (type is null)
                {
                    continue;
                }

                // If the variable doesn't have a name, it is invalid.
                if (string.IsNullOrEmpty(symbol.Name))
                {
                    continue;
                }

                if (!TryGetVariableStyle(symbolMap, symbol, model, type,
                    captured, dataFlowIn, dataFlowOut, alwaysAssigned, variableDeclared,
                    readInside, writtenInside, readOutside, writtenOutside, unsafeAddressTaken,
                    out var variableStyle))
                {
                    continue;
                }

                AddVariableToMap(variableInfoMap, symbol, CreateFromSymbol(model.Compilation, symbol, type, variableStyle, variableDeclared));
            }
        }

        private bool TryGetVariableStyle(
            Dictionary<ISymbol, List<SyntaxToken>> symbolMap,
            ISymbol symbol,
            SemanticModel model,
            ITypeSymbol type,
            bool captured,
            bool dataFlowIn,
            bool dataFlowOut,
            bool alwaysAssigned,
            bool variableDeclared,
            bool readInside,
            bool writtenInside,
            bool readOutside,
            bool writtenOutside,
            bool unsafeAddressTaken,
            out VariableStyle variableStyle)
        {
            if (!ExtractMethodMatrix.TryGetVariableStyle(
                    dataFlowIn, dataFlowOut, alwaysAssigned, variableDeclared,
                    readInside, writtenInside, readOutside, writtenOutside, unsafeAddressTaken,
                    out variableStyle))
            {
                return false;
            }

            if (UserDefinedValueType(model.Compilation, type))
            {
                variableStyle = AlwaysReturn(variableStyle);
                return true;
            }

            // for captured variable, never try to move the decl into extracted method
            if (captured && variableStyle == VariableStyle.MoveIn)
            {
                variableStyle = VariableStyle.Out;
                return true;
            }

            // don't blindly always return. make sure there is a write inside of the selection
            if (!writtenInside)
            {
                return true;
            }

            variableStyle = AlwaysReturn(variableStyle);
            return true;
        }

        private bool UserDefinedValueType(Compilation compilation, ITypeSymbol type)
        {
            if (!type.IsValueType || type.IsPointerType() || type.IsEnumType())
            {
                return false;
            }

            return type.OriginalDefinition.SpecialType == SpecialType.None && !WellKnownFrameworkValueType(compilation, type);
        }

        private bool WellKnownFrameworkValueType(Compilation compilation, ITypeSymbol type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            var cancellationTokenType = compilation.GetTypeByMetadataName(typeof(CancellationToken).FullName);
            if (cancellationTokenType != null && cancellationTokenType.Equals(type))
            {
                return true;
            }

            return false;
        }

        protected VariableStyle AlwaysReturn(VariableStyle style)
        {
            if (style == VariableStyle.InputOnly)
            {
                return VariableStyle.Ref;
            }

            if (style == VariableStyle.MoveIn)
            {
                return VariableStyle.Out;
            }

            if (style == VariableStyle.SplitIn)
            {
                return VariableStyle.Out;
            }

            if (style == VariableStyle.SplitOut)
            {
                return VariableStyle.OutWithMoveOut;
            }

            return style;
        }

        private void AddVariableToMap(IDictionary<ISymbol, VariableInfo> variableInfoMap, ISymbol localOrParameter, VariableInfo variableInfo)
            => variableInfoMap.Add(localOrParameter, variableInfo);

        private VariableInfo CreateFromSymbol(
            Compilation compilation,
            ISymbol symbol,
            ITypeSymbol type,
            VariableStyle style,
            bool variableDeclared)
        {
            return CreateFromSymbolCommon<LocalDeclarationStatementSyntax>(compilation, symbol, type, style, _sNonNoisySyntaxKindSet);
        }

        private VariableInfo CreateFromSymbolCommon<T>(
            Compilation compilation,
            ISymbol symbol,
            ITypeSymbol type,
            VariableStyle style,
            HashSet<int> nonNoisySyntaxKindSet) where T : SyntaxNode
        {
            switch (symbol)
            {
                case ILocalSymbol local:
                    return new VariableInfo(
                        new LocalVariableSymbol<T>(compilation, local, type, nonNoisySyntaxKindSet),
                        style);
                case IParameterSymbol parameter:
                    return new VariableInfo(new ParameterVariableSymbol(compilation, parameter, type), style);
                default:
                    return null;
            }
        }

        private ITypeSymbol GetSymbolType(ISymbol symbol)
        {
            switch (symbol)
            {
                case ILocalSymbol local:
                    return local.Type;
                case IParameterSymbol parameter:
                    return parameter.Type;
                default:
                    return null;
            }
        }

        private bool IsInExpressionOrHasReturnStatement(SemanticModel model)
        {
            var containsReturnStatement = ContainsReturnStatementInSelectedCode(model);
            return containsReturnStatement;
        }

        private bool ContainsReturnStatementInSelectedCode(SemanticModel model)
        {
            var dataFlowAnalysis = _semanticModel.AnalyzeControlFlow(_selectionResult.FirstStatement(), _selectionResult.LastStatement());
            return ContainsReturnStatementInSelectedCode(dataFlowAnalysis.ExitPoints);
        }

        private bool ContainsReturnStatementInSelectedCode(IEnumerable<SyntaxNode> jumpOutOfRegionStatements)
            => jumpOutOfRegionStatements.Any(n => n is ReturnStatementSyntax);

        private (IList<VariableInfo> parameters, ITypeSymbol returnType, VariableInfo variableToUseAsReturnValue)
            GetSignatureInformation(
                DataFlowAnalysis dataFlowAnalysisData,
                IDictionary<ISymbol, VariableInfo> variableInfoMap,
                bool isInExpressionOrHasReturnStatement)
        {
            var model = _semanticModel;
            var compilation = model.Compilation;
            if (isInExpressionOrHasReturnStatement)
            {
                // check whether current selection contains return statement
                var parameters = GetMethodParameters(variableInfoMap.Values);
                var returnType = _selectionResult.GetContainingScopeType(_semanticModel) ?? compilation.GetSpecialType(SpecialType.System_Object);

                return (parameters, returnType, (VariableInfo)null);
            }
            else
            {
                // no return statement
                var parameters = MarkVariableInfoToUseAsReturnValueIfPossible(GetMethodParameters(variableInfoMap.Values));
                var variableToUseAsReturnValue = parameters.FirstOrDefault(v => v.UseAsReturnValue);

                var returnType = variableToUseAsReturnValue != null
                    ? variableToUseAsReturnValue.Type
                    : compilation.GetSpecialType(SpecialType.System_Void);

                return (parameters, returnType, variableToUseAsReturnValue);
            }
        }

        private IList<VariableInfo> GetMethodParameters(ICollection<VariableInfo> variableInfo)
        {
            var list = new List<VariableInfo>(variableInfo);
            VariableInfo.SortVariables(_semanticModel.Compilation, list);
            return list;
        }

        private IList<VariableInfo> MarkVariableInfoToUseAsReturnValueIfPossible(IList<VariableInfo> variableInfo)
        {
            var variableToUseAsReturnValueIndex = GetIndexOfVariableInfoToUseAsReturnValue(variableInfo);
            if (variableToUseAsReturnValueIndex >= 0)
            {
                variableInfo[variableToUseAsReturnValueIndex] = VariableInfo.CreateReturnValue(variableInfo[variableToUseAsReturnValueIndex]);
            }

            return variableInfo;
        }

        private int GetIndexOfVariableInfoToUseAsReturnValue(IList<VariableInfo> variableInfo)
        {
            var numberOfOutParameters = 0;
            var numberOfRefParameters = 0;

            var outSymbolIndex = -1;
            var refSymbolIndex = -1;

            for (var i = 0; i < variableInfo.Count; i++)
            {
                var variable = variableInfo[i];

                // there should be no-one set as return value yet
                if (!variable.CanBeUsedAsReturnValue)
                {
                    continue;
                }

                // check modifier
                if (variable.ParameterModifier == ParameterBehavior.Ref)
                {
                    numberOfRefParameters++;
                    refSymbolIndex = i;
                }
                else if (variable.ParameterModifier == ParameterBehavior.Out)
                {
                    numberOfOutParameters++;
                    outSymbolIndex = i;
                }
            }

            // if there is only one "out" or "ref", that will be converted to return statement.
            if (numberOfOutParameters == 1)
            {
                return outSymbolIndex;
            }

            if (numberOfRefParameters == 1)
            {
                return refSymbolIndex;
            }

            return -1;
        }
    }
}
