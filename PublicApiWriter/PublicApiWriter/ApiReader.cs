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
            string signature = SymbolFormatter.GetSignature(symbol);
            var memberImportance = symbol.GetImportance();
            var apiNode = assemblyNode.AddMember(signature, symbolNamespace, symbol.DeclaredAccessibility, symbol.Kind, symbol.Name, memberImportance);
            AddMembers(apiNode, symbol, cancellationToken);
            return apiNode;
        }

        private void AddMembers(ApiNode parent, ISymbol symbol, CancellationToken cancellationToken)
        {
            AddMembers(parent, symbol as INamespaceOrTypeSymbol, cancellationToken);
            AddEventMembers(parent, symbol as IEventSymbol, cancellationToken);
            AddPropertyMembers(parent, symbol as IPropertySymbol, cancellationToken);
        }

        private void AddMembers(ApiNode parent, INamespaceOrTypeSymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol == null || HasBoringAlwaysIdenticalMembers(symbol)) return;
            foreach (var childSymbol in GetMembers(symbol))
            {
                var childNode = CreateApiNode(parent, childSymbol, cancellationToken);
            }
        }

        private static bool HasBoringAlwaysIdenticalMembers(INamespaceOrTypeSymbol symbol)
        {
            return (symbol as ITypeSymbol)?.TypeKind == TypeKind.Delegate;
        }

        private void AddEventMembers(ApiNode parent, IEventSymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol == null) return;
            foreach (var childSymbol in new[] { symbol.AddMethod, symbol.RemoveMethod, symbol.RaiseMethod }.Where(x => x != null))
            {
                var childNode = CreateApiNode(parent, childSymbol, cancellationToken);
            }
        }


        private void AddPropertyMembers(ApiNode parent, IPropertySymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol == null) return;
            foreach (var childSymbol in new[] { symbol.GetMethod, symbol.SetMethod }.Where(x => x != null))
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