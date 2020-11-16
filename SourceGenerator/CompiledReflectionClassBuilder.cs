﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    internal class CompiledReflectionClassBuilder
    {
        private const string CompiledReflectionStub = @"
using System.Collections.Generic;
using System.Linq;

// <auto-generated />
public static partial class CompiledReflection
{

    public partial static IEnumerable<string> GetPropertyNames<T>() => Enumerable.Empty<string>();
    public partial static IEnumerable<CompiledPropertyInfo> GetPropertyInfo<T>() => Enumerable.Empty<CompiledPropertyInfo>();
}
";
        private const string CompiledReflectionBase = @"
using System.Collections.Generic;
using System.Linq;

// <auto-generated />
public static partial class CompiledReflection
{
    public static partial IEnumerable<string> GetPropertyNames<T>();
    public static partial IEnumerable<CompiledPropertyInfo> GetPropertyInfo<T>();
}
";

        private readonly GeneratorExecutionContext _context;

        private readonly HashSet<ITypeSymbol> _getPropertyNamesCalls = new HashSet<ITypeSymbol>();
        private readonly HashSet<ITypeSymbol> _getPropertyInfoCalls = new HashSet<ITypeSymbol>();

        public CompiledReflectionClassBuilder(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void BuildPartialContractClass()
        {
            var sourceText = SourceText.From(CompiledReflectionBase, Encoding.UTF8);
            _context.AddSource("CompiledReflection.partial.cs", sourceText);
        }

        public void AddSyntaxTree(ref Compilation compilation, CSharpParseOptions options)
        {
            var sourceText = SourceText.From(CompiledReflectionBase, Encoding.UTF8);
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceText, options));
        }

        public void BuildStub()
        {
            _context.AddSource("CompiledReflection.cs", CompiledReflectionStub);
        }

        public void CheckAndRegisterCalls(IMethodSymbol methodSymbol)
        {
            var typeArgument = methodSymbol.TypeArguments.First();

            if (methodSymbol.Name.Contains("GetPropertyNames"))
            {
                if (!_getPropertyNamesCalls.Contains(typeArgument))
                {
                    _getPropertyNamesCalls.Add(typeArgument);
                }
            }
            else if (methodSymbol.Name.Contains("GetPropertyInfo"))
            {
                if (!_getPropertyInfoCalls.Contains(typeArgument))
                {
                    _getPropertyInfoCalls.Add(typeArgument);
                }
            }
        }

        public void Build()
        {
            var sb = new StringBuilder();
            sb.Append(@"
using System.Collections.Generic;
using System.Linq;

// <auto-generated />
public static partial class CompiledReflection
{"
);

            CreateGetPropertyNames(sb, _getPropertyNamesCalls);
            sb.AppendLine();
            CreateGetPropertyInfo(sb, _getPropertyInfoCalls);


            sb.AppendLine();
            sb.AppendLine("}");

            string compiledReflectionClass = sb.ToString();
            _context.AddSource("CompiledReflection.cs", compiledReflectionClass);
        }

        private static void CreateGetPropertyInfo(StringBuilder sb, HashSet<ITypeSymbol> getPropertyInfoCalls)
        {
            sb.Append(@"
    public static partial IEnumerable<CompiledPropertyInfo> GetPropertyInfo<T>()
    {
        var wrapper = new TypeWrapper<T>();

        return wrapper switch
        {"
            );

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                sb.AppendLine($"\t\t\tTypeWrapper<{typeName}> w => GetPropertyInfo(w),");
            }

            sb.Append(@"
            _ => throw new System.NotImplementedException()
            };
        }"
            );

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);

                var properties = type.GetMembers().Where(s => s.Kind == SymbolKind.Property && !s.IsStatic).OfType<IPropertySymbol>().ToArray();
                var propertyNames = type.GetMembers().Where(s => s.Kind == SymbolKind.Property && !s.IsStatic).Select(s => s.Name).ToArray();

                sb.AppendLine(@$"
    private static IEnumerable<CompiledPropertyInfo> GetPropertyInfo(TypeWrapper<{typeName}> wrapper)
    {{
        return new List<CompiledPropertyInfo>
        {{"
                );

                foreach (var property in properties)
                {
                    sb.AppendLine($"\t\t\tnew CompiledPropertyInfo(\"{property.Name}\", \"{property.Type.ToDisplayString(Constants.SymbolDisplayFormat)}\"),");
                }

                sb.AppendLine("\t\t};}");
            }
        }

        private static void CreateGetPropertyNames(StringBuilder sb, HashSet<ITypeSymbol> typesWhereGetPropertyNamesWasCalled)
        {
            sb.Append(@"
    public static partial IEnumerable<string> GetPropertyNames<T>()
    {
        var wrapper = new TypeWrapper<T>();

        return wrapper switch
        {"
            );

            foreach (var type in typesWhereGetPropertyNamesWasCalled)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                sb.AppendLine($"\t\t\tTypeWrapper<{typeName}> w => GetPropertyNames(w),");
            }

            sb.Append(@"
            _ => throw new System.NotImplementedException()
            };
        }"
            );

            foreach (var type in typesWhereGetPropertyNamesWasCalled)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);

                var propertyNames = type.GetMembers().Where(s => s.Kind == SymbolKind.Property && !s.IsStatic).Select(s => s.Name).ToArray();

                sb.AppendLine(@$"
    private static IEnumerable<string> GetPropertyNames(TypeWrapper<{typeName}> wrapper)
    {{
        return new List<string>
        {{"
                );

                foreach (string propertyName in propertyNames)
                {
                    sb.AppendLine("\t\t\t\"" + propertyName + "\",");
                }

                sb.AppendLine("\t\t};}");
            }
        }
    }
}