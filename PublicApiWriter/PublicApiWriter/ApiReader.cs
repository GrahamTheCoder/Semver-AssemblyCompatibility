using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace PublicApiWriter
{
    internal class ApiReader
    {
        private static SymbolDisplayFormat s_Format = CreateSignatureFormat();
        private readonly Solution m_Solution;

        public ApiReader(Solution solution)
        {
            m_Solution = solution;
        }

        public IEnumerable<ApiNode> ReadProjects(CancellationToken token)
        {
            foreach (var project in m_Solution.Projects)
            {
                var assemblyNode = ApiNode.CreateAssemblyRoot(project.AssemblyName);
                AddTypes(project, assemblyNode, token).Wait(token);
                yield return assemblyNode;
            }
        }

        private async Task AddTypes(Project project, ApiNode assemblyNode, CancellationToken cancellationToken)
        {
            foreach (var document in project.Documents)
            {
                var tree = await document.GetSyntaxTreeAsync(cancellationToken);
                var root = await tree.GetRootAsync(cancellationToken);
                var semantic = await document.GetSemanticModelAsync(cancellationToken);
                var classes = root.ChildNodes();
                var semanticDocumentMembers = classes
                    .Select(syntaxNode => semantic.GetDeclaredSymbol(syntaxNode))
                    .Where(symbol => symbol != null);
                foreach (var semanticDocSymbol in semanticDocumentMembers)
                {
                    CreateApiNode(assemblyNode, semanticDocSymbol, cancellationToken);
                }
            }
        }

        private ApiNode CreateApiNode(ApiNode assemblyNode, ISymbol symbol, CancellationToken cancellationToken)
        {
            var symbolNamespace = symbol.ContainingNamespace.Name;
            string signature = GetSignature(symbol);
            var memberImportance = GetImportance(assemblyNode, symbol);
            var apiNode = assemblyNode.AddMember(signature, symbol.Name, symbolNamespace, symbol.DeclaredAccessibility, memberImportance);
            AddMembers(apiNode, symbol as INamespaceOrTypeSymbol, cancellationToken);
            return apiNode;
        }

        private static string GetSignature(ISymbol symbol)
        {
            return SymbolDisplay.ToDisplayString(symbol, s_Format);
        }

        private static int GetImportance(ApiNode assemblyNode, ISymbol symbol)
        {
            var method = symbol as IMethodSymbol;
            var methodKindsByIncreasingImportance = new List<bool>
            {
                symbol is IEventSymbol,
                symbol is IPropertySymbol,
                method?.MethodKind == MethodKind.Destructor,
                method?.MethodKind == MethodKind.Constructor,
                method?.MethodKind == MethodKind.StaticConstructor,
            };

            var type = symbol as ITypeSymbol;
            var typeKindsByIncreasingImportance = new List<TypeKind?>
            {
                TypeKind.Class,
                TypeKind.Enum,
                TypeKind.Struct,
                TypeKind.Interface,
            };

            return methodKindsByIncreasingImportance.IndexOf(true)
                + typeKindsByIncreasingImportance.IndexOf(type?.TypeKind);
        }

        private static SymbolDisplayFormat CreateSignatureFormat()
        {
            var defaultFormat = SymbolDisplayFormat.CSharpErrorMessageFormat;
            return
                defaultFormat
                .WithMemberOptions(SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeAccessibility | SymbolDisplayMemberOptions.IncludeModifiers | SymbolDisplayMemberOptions.IncludeType)
                .WithKindOptions(SymbolDisplayKindOptions.IncludeMemberKeyword | SymbolDisplayKindOptions.IncludeNamespaceKeyword | SymbolDisplayKindOptions.IncludeTypeKeyword)
                .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance)
                .WithParameterOptions(SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType);
        }

        private void AddMembers(ApiNode parent, INamespaceOrTypeSymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol == null) return;
            foreach (var childSymbol in GetMembers(symbol))
            {
                var childNode = CreateApiNode(parent, childSymbol, cancellationToken);
            }
        }

        private static IEnumerable<ISymbol> GetMembers(INamespaceOrTypeSymbol symbol)
        {
            return from member in symbol.GetMembers()
                   let methodKind = (member as IMethodSymbol)?.MethodKind
                   where methodKind != MethodKind.PropertyGet && methodKind != MethodKind.PropertySet
                   where methodKind != MethodKind.EventAdd && methodKind != MethodKind.EventRemove
                   select member;
        }
    }
}