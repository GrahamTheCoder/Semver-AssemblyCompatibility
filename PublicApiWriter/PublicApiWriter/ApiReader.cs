using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PublicApiWriter
{
    internal class ApiReader
    {
        private readonly Solution m_Solution;

        public ApiReader(Solution solution)
        {
            m_Solution = solution;
        }

        internal async Task WritePublicMembers(string outputFile, PrinterConfig printerConfig, CancellationToken cancellationToken)
        {
            var solutionNode = ReadProjects(cancellationToken);
            await WritePublicApi(solutionNode, printerConfig, outputFile, cancellationToken);
        }

        private async Task WritePublicApi(IEnumerable<ApiNode> assemblies, PrinterConfig printerConfig, string outputFile, CancellationToken cancellationToken)
        {
            using (var file = new StreamWriter(outputFile, false))
            {
                foreach (var assembly in assemblies)
                {
                    await assembly.Write(file, printerConfig, cancellationToken);
                }
            }
        }

        private IEnumerable<ApiNode> ReadProjects(CancellationToken token)
        {
            foreach (var project in m_Solution.Projects)
            {
                var assemblyNode = new ApiNode(project.AssemblyName, "assembly", Accessibility.Public);
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
                var apiNodes = classes
                    .Select(syntaxNode => semantic.GetDeclaredSymbol(syntaxNode))
                    .Where(symbol => symbol != null)
                    .Select(symbol => CreateApiNode(symbol, cancellationToken));
                foreach (var child in apiNodes)
                {
                    assemblyNode.AddMember(child);
                }
            }
        }

        private ApiNode CreateApiNode(ISymbol symbol, CancellationToken cancellationToken)
        {
            var symbolAccessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
            var symbolNamespace = symbol.ContainingNamespace.Name;
            var symbolKind = symbol.Kind.ToString().ToLowerInvariant();
            var typeDescription = $"{symbolAccessibility} {symbolKind} {symbol}";
            var apiNode = new ApiNode(typeDescription, symbolNamespace, symbol.DeclaredAccessibility);
            AddMembers(apiNode, symbol as INamespaceOrTypeSymbol, cancellationToken);
            return apiNode;
        }

        private void AddMembers(ApiNode parent, INamespaceOrTypeSymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol == null) return;
            foreach (var childSymbol in symbol.GetMembers())
            {
                var childNode = CreateApiNode(childSymbol, cancellationToken);
                parent.AddMember(childNode);
            }
        }
    }
}