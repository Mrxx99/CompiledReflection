﻿using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        private const string TypeWrapper = @"
// <auto-generated />
public class TypeWrapper<T>
{

}";

        private const string Accessibility = @"
// <auto-generated />
public enum Accessibility
{
    Private,
    Protected,
    Internal,
    Public
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CompiledReflectionSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            if (context.SyntaxReceiver is not CompiledReflectionSyntaxReceiver receiver)
            {
                return;
            }

            var compiledReflectionClassBuilder = new CompiledReflectionClassBuilder(context);
            var compiledPropertyInfoClassBuilder = new CompiledPropertyInfoClassBuilder(context);

            context.AddSource("TypeWrapper.cs", SourceText.From(TypeWrapper, Encoding.UTF8));
            context.AddSource("Accessibility.cs", SourceText.From(Accessibility, Encoding.UTF8));
            compiledReflectionClassBuilder.BuildPartialContractClass();
            compiledPropertyInfoClassBuilder.BuildPartialContractClass();

            if (!receiver.ExpressionsUsingCompiledReflection.Any())
            {
                compiledReflectionClassBuilder.BuildStub();
                compiledPropertyInfoClassBuilder.BuildStub();
                return;
            }

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation;

            compiledReflectionClassBuilder.AddSyntaxTree(ref compilation, options);
            compiledPropertyInfoClassBuilder.AddSyntaxTree(ref compilation, options);

            foreach (var compiledReflectionExpression in receiver.ExpressionsUsingCompiledReflection)
            {
                var model = compilation.GetSemanticModel(compiledReflectionExpression.SyntaxTree);
                var methodSymbol = model.GetSymbolInfo(compiledReflectionExpression.Expression).Symbol as IMethodSymbol;

                if (methodSymbol == null)
                {
                    continue;
                }

                compiledReflectionClassBuilder.CheckAndRegisterCalls(methodSymbol);
                compiledPropertyInfoClassBuilder.CheckAndRegisterCalls(methodSymbol);
            }

            compiledPropertyInfoClassBuilder.Build();
            compiledReflectionClassBuilder.Build();
        }
    }
}
