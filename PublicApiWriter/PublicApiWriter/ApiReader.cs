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

        internal async Task WritePublicMembers(string outputFile, CancellationToken cancellationToken)
        {
            var solutionNode = ReadProjects(cancellationToken);
            await WritePublicApi(solutionNode, outputFile, cancellationToken);
        }

        private async Task WritePublicApi(IEnumerable<ApiNode> assemblies, string outputFile, CancellationToken cancellationToken)
        {
            using (var file = new StreamWriter(outputFile, false))
            {
                foreach (var assembly in assemblies)
                {
                    await assembly.Write(file, Accessibility.Public, cancellationToken);
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
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classNode in classes)
                {
                    var child = CreateApiNode(semantic.GetDeclaredSymbol(classNode) as ITypeSymbol, cancellationToken);
                    assemblyNode.AddMember(child);
                }
            }
        }

        private ApiNode CreateApiNode(ISymbol symbol, CancellationToken cancellationToken)
        {

            var symbolAccessibility = symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
            var symbolNamespace = symbol.ContainingNamespace.Name;
            var symbolName = symbol.MetadataName;
            var symbolKind = symbol.Kind;
            var extraInfo = GetAfterNameInfo(symbol);
            var typeDescription = $"{symbolAccessibility} {symbolKind} {symbolNamespace}.{symbolName}{extraInfo}";
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

        private static string GetAfterNameInfo(ISymbol symbol)
        {
            if (symbol is ITypeSymbol) return GetAfterNameInfo((ITypeSymbol) symbol);
            return "";
        }

        private static string GetAfterNameInfo(ITypeSymbol symbol)
        {
            var interfaces = symbol.AllInterfaces.Any()
                ? ", " + string.Join(", ", symbol.AllInterfaces)
                : "";
            return $" : {symbol.BaseType}{interfaces}";
        }
    }
}