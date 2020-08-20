using Microsoft.CodeAnalysis;

namespace GenSharp.Refactorings.Analyzers
{
    public static class DiagnosticDescriptors
    {
        /// <summary>GS01</summary>
        public static readonly DiagnosticDescriptor ExtractStatement = new DiagnosticDescriptor(
            id:                 DiagnosticIdentifiers.ExtractStatement, 
            title:              "Extract Statement.", 
            messageFormat:      "Extract Statement.", 
            category:           "Extract", 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true);
    }
}
