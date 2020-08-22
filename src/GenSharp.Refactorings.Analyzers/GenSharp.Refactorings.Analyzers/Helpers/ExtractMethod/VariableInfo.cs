using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class VariableInfo
    {
        private readonly VariableSymbol _variableSymbol;
        private readonly VariableStyle _variableStyle;
        private readonly bool _useAsReturnValue;

        public VariableInfo(
            VariableSymbol variableSymbol,
            VariableStyle variableStyle, 
            bool useAsReturnValue = false)
        {
            _variableSymbol = variableSymbol;
            _variableStyle = variableStyle;
            _useAsReturnValue = useAsReturnValue;
        }

        public static VariableInfo CreateReturnValue(VariableInfo variable)
        {
            if (!variable.CanBeUsedAsReturnValue)
            {
                return variable;
            }

            if (variable.ParameterModifier != ParameterBehavior.Out && variable.ParameterModifier != ParameterBehavior.Ref)
            {
                return variable;
            }

            return new VariableInfo(variable._variableSymbol, variable._variableStyle, useAsReturnValue: true);
        }

        public string Name => _variableSymbol.Name;

        public ITypeSymbol Type => _variableSymbol.OriginalType;
        
        public bool UseAsReturnValue => _useAsReturnValue;

        public bool CanBeUsedAsReturnValue => _variableStyle.ReturnStyle.ReturnBehavior != ReturnBehavior.None;

        public bool UseAsParameter =>
            (!_useAsReturnValue && _variableStyle.ParameterStyle.ParameterBehavior != ParameterBehavior.None) ||
            (_useAsReturnValue && _variableStyle.ReturnStyle.ParameterBehavior != ParameterBehavior.None);

        public ParameterBehavior ParameterModifier => _useAsReturnValue ? _variableStyle.ReturnStyle.ParameterBehavior : _variableStyle.ParameterStyle.ParameterBehavior;

        public static void SortVariables(Compilation compilation, List<VariableInfo> list)
        {
            var cancellationTokenType = compilation.GetTypeByMetadataName(typeof(CancellationToken).FullName);
            list.Sort((v1, v2) => Compare(v1, v2, cancellationTokenType));
        }

        private static int Compare(VariableInfo left, VariableInfo right, INamedTypeSymbol cancellationTokenType)
            => VariableSymbol.Compare(left._variableSymbol, right._variableSymbol, cancellationTokenType);
    }
}
