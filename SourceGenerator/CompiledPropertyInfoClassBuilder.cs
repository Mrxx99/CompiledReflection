﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    internal class CompiledPropertyInfoClassBuilder
    {
        private const string CompiledPropertyInfoStub = @"
using System;

// <auto-generated />
public partial class CompiledPropertyInfo
{
    public partial object GetValue(object instance) => null;
    public partial bool TrySetValue(object instance, object value) => false;
}";

        private const string CompiledPropertyInfoBase = @"
using System;

// <auto-generated />
public partial class CompiledPropertyInfo
{
    public string Name { get; }
    public string TypeName { get; }

    public CompiledPropertyInfo(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public partial object GetValue(object instance);
    public partial bool TrySetValue(object instance, object value);
}";

        private readonly GeneratorExecutionContext _context;

        private readonly HashSet<ITypeSymbol> _getPropertyInfoCalls = new HashSet<ITypeSymbol>();

        public CompiledPropertyInfoClassBuilder(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void BuildPartialContractClass()
        {
            var sourceText = SourceText.From(CompiledPropertyInfoBase, Encoding.UTF8);
            _context.AddSource("CompiledPropertyInfo.partial.cs", sourceText);
        }

        public void AddSyntaxTree(ref Compilation compilation, CSharpParseOptions options)
        {
            var sourceText = SourceText.From(CompiledPropertyInfoBase, Encoding.UTF8);
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceText, options));
        }

        public void BuildStub()
        {
            _context.AddSource("CompiledPropertyInfo.cs", CompiledPropertyInfoStub);
        }

        public void CheckAndRegisterCalls(IMethodSymbol methodSymbol)
        {
            var typeArgument = methodSymbol.TypeArguments.First();

            if (methodSymbol.Name.Contains("GetPropertyInfo"))
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

            StartClass(sb);

            //System.Diagnostics.Debugger.Launch();

            CreateGetValueMethods(sb, _getPropertyInfoCalls);
            CreateTrySetValueMethods(sb, _getPropertyInfoCalls);

            sb.AppendLine("}");

            string compiledPropertyInfoClass = sb.ToString();
            _context.AddSource("CompiledPropertyInfo.cs", compiledPropertyInfoClass);
        }

        private void CreateTrySetValueMethods(StringBuilder sb, HashSet<ITypeSymbol> getPropertyInfoCalls)
        {
            sb.Append(@"
    public partial bool TrySetValue(object instance, object value)
    {
        return instance switch
        {"
            );

            sb.AppendLine();

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                sb.AppendLine(@$"           {typeName} i => TrySetValueInternal(i, value),");
            }

            sb.AppendLine(@"            _ => false");
            sb.AppendLine("         };");
            sb.AppendLine("     }");

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                var properties = type.GetMembers().Where(s => s.Kind == SymbolKind.Property && !s.IsStatic).OfType<IPropertySymbol>().ToArray();

                sb.AppendLine(@$"
    private bool TrySetValueInternal({typeName} instance, object value)
    {{
        bool success = true;
        switch ((Name, value))
        {{"
                );

                sb.AppendLine();

                foreach (var property in properties)
                {
                    var setMethod = property.SetMethod;

                    if (setMethod is null || setMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected || setMethod.IsInitOnly)
                    {
                        continue;
                    }

                    string propertyType = property.Type.ToDisplayString(Constants.SymbolDisplayFormat);
                    sb.AppendLine($"            case (\"{property.Name}\", {propertyType} v): instance.{property.Name} = v; break;");
                }

                sb.AppendLine("         default: success = false; break;");

                sb.AppendLine("         }");
                sb.AppendLine("         return success;");
                sb.AppendLine("     }");

            }
        }

        private static void StartClass(StringBuilder sb)
        {
            sb.Append(@"
using System.Collections.Generic;
using System.Linq;

// <auto-generated />
public partial class CompiledPropertyInfo
{"
);
        }

        private static void CreateGetValueMethods(StringBuilder sb, HashSet<ITypeSymbol> getPropertyInfoCalls)
        {
            sb.Append(@"
    public partial object GetValue(object instance)
    {
        return instance switch
        {"
            );

            sb.AppendLine();

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                sb.AppendLine(@$"           {typeName} i => GetValue(i),");
            }

            sb.AppendLine(@"            _ => throw new System.NotImplementedException()
        };
    }"
            );

            foreach (var type in getPropertyInfoCalls)
            {
                var typeName = type.ToDisplayString(Constants.SymbolDisplayFormat);
                var properties = type.GetMembers().Where(s => s.Kind == SymbolKind.Property && !s.IsStatic).OfType<IPropertySymbol>().ToArray();

                sb.AppendLine(@$"
    private object GetValue({typeName} instance)
    {{
        return Name switch
        {{"
                );

                foreach (var property in properties)
                {
                    var getMethod = property.GetMethod;

                    if (getMethod is null || getMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected)
                    {
                        continue;
                    }

                    sb.AppendLine($"            \"{property.Name}\" => instance.{property.Name},");
                }

                sb.AppendLine($"            _ => null");

                sb.AppendLine("         };");
                sb.AppendLine("     }");
            }
        }
    }
}
