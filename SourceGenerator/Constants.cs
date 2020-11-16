using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    public static class Constants
    {
        public static readonly SymbolDisplayFormat SymbolDisplayFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
    }
}
