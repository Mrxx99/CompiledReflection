using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    public static class Helpers
    {
        public static string GetAccessibiltyString(this Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Private => AccessibilityStrings.Private,
                Accessibility.ProtectedAndInternal or Accessibility.Protected or Accessibility.ProtectedOrInternal => AccessibilityStrings.Protected,
                Accessibility.Internal => AccessibilityStrings.Interanl,
                Accessibility.Public => AccessibilityStrings.Public,
                _ => null,
            };
        }
    }

    public struct AccessibilityStrings
    {
        public const string Accessibility = "Accessibility";
        public const string Private = "Private";
        public const string Protected = "Protected";
        public const string Interanl = "Internal";
        public const string Public = "Public";
    }
}
