using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssemblyApi.SymbolExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace AssemblyApi.ModelBuilder
{
    internal class ApiReader
    {
        private readonly Solution m_Solution;
        private static readonly Dictionary<string, string> s_DefaultProperties = new Dictionary<string, string>
        {
            {"BuildingInsideVisualStudio", "true"},
            {"SemanticAnalysisOnly", "true"},
        };

        public ApiReader(Solution solution)
        {
            m_Solution = solution;
        }

        public static async Task<IReadOnlyCollection<ApiNode>> ReadApiFromSolution(string solutionFilePath, CancellationToken cancellationToken)
        {
            using (var msWorkspace = MSBuildWorkspace.Create(s_DefaultProperties))
            {
                var result = await msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken);
                var solution = new ApiReader(result);
                return await solution.ReadProjects(cancellationToken);
            }
        }

        public async Task<IReadOnlyCollection<ApiNode>> ReadProjects(CancellationToken token)
        {
            var projectNodes = m_Solution.Projects.Select(project => CreateAssemblyNode(token, project));
            return await Task.WhenAll(projectNodes);
        }

        private async Task<ApiNode> CreateAssemblyNode(CancellationToken token, Project project)
        {
            var assemblyNode = ApiNode.CreateAssemblyRoot(project.AssemblyName);
            await AddTypes(project, assemblyNode, token);
            return assemblyNode;
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
            string signature = symbol.GetSignature();
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