using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers
{
    internal static class DiagnosticExtensions
    {
        public static Diagnostic FindExtractMethodDiagnostic(this IEnumerable<Diagnostic> diagnostics) 
            => diagnostics.FirstOrDefault(d => d.Id == DiagnosticIdentifiers.ExtractMethod);
    }
}
