using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using PublicApiWriter.SymbolExtensions;

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
            var apiNode = assemblyNode.AddMember(signature, symbolNamespace, GetPresentedAccessibility(symbol), symbol.Kind, symbol.Name, memberImportance);
            AddMembers(apiNode, symbol, cancellationToken);
            return apiNode;
        }

        private static Accessibility GetPresentedAccessibility(ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Field && symbol.ContainingType.TypeKind == TypeKind.Enum
                ? Accessibility.NotApplicable
                : symbol.DeclaredAccessibility;
        }

        private void AddMembers(ApiNode parent, ISymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var childSymbol in symbol.GetApiAffectingMembers())
            {
                var childNode = CreateApiNode(parent, childSymbol, cancellationToken);
            }
        }
    }
}