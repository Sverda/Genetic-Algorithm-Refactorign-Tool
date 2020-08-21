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
            defaultSeverity:    DiagnosticSeverity.Hidden, 
            isEnabledByDefault: true);

        /// <summary>GS02</summary>
        public static readonly DiagnosticDescriptor ExtractMethod = new DiagnosticDescriptor(
            id:                 DiagnosticIdentifiers.ExtractMethod, 
            title:              "Extract Method.", 
            messageFormat:      "Extract Method.", 
            category:           "Extract", 
            defaultSeverity:    DiagnosticSeverity.Hidden, 
            isEnabledByDefault: true);
    }
}
