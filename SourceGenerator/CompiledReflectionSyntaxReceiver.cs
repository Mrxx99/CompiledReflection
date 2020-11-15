using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator
{
    public class CompiledReflectionSyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> ExpressionsUsingCompiledReflection { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.ToFullString().Contains("CompiledReflection"))
            {
                ExpressionsUsingCompiledReflection.Add(invocationExpression);
            }
        }
    }
}
