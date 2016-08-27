using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtc.AssemblyApi.ModelBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Accessibility = Gtc.AssemblyApi.Model.Accessibility;
using SymbolKind = Gtc.AssemblyApi.Model.SymbolKind;
using MsAccessibility = Microsoft.CodeAnalysis.Accessibility;
using MsSymbolKind = Microsoft.CodeAnalysis.SymbolKind;

namespace Gtc.AssemblyApi.CodeAnalysis
{
    internal class ApiReader
    {
        private readonly IEnumerable<Project> m_Projects;
        private static readonly Dictionary<string, string> s_DefaultProperties = new Dictionary<string, string>
        {
            {"BuildingInsideVisualStudio", "true"},
            {"SemanticAnalysisOnly", "true"},
        };

        private static readonly MsSymbolKind[] NonApiSymbolKinds = { MsSymbolKind.Alias , MsSymbolKind.Preprocessing, MsSymbolKind.Label};

        public ApiReader(IEnumerable<Project> projects)
        {
            m_Projects = projects;
        }

        public static async Task<IReadOnlyCollection<IApiNode>> ReadApiFromProjects(string solutionFilePath, CancellationToken cancellationToken)
        {
            using (var msWorkspace = MSBuildWorkspace.Create(s_DefaultProperties))
            {
                var projects = await GetProjects(solutionFilePath, cancellationToken, msWorkspace);
                var solution = new ApiReader(projects);
                return await solution.ReadProjects(cancellationToken);
            }
        }

        private static async Task<IEnumerable<Project>>  GetProjects(string solutionFilePath, CancellationToken cancellationToken,
            MSBuildWorkspace msWorkspace)
        {
            if (solutionFilePath.EndsWith(".sln"))
            {
                var solution = await msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken);
                return solution.Projects;
            }
            return new [] { await msWorkspace.OpenProjectAsync(solutionFilePath, cancellationToken)};
        }

        public async Task<IReadOnlyCollection<IApiNode>> ReadProjects(CancellationToken token)
        {
            var projectNodes = m_Projects.Select(project => CreateAssemblyNode(token, project));
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
                foreach (var semanticDocSymbol in semanticDocumentMembers.Where(m => !NonApiSymbolKinds.Contains(m.Kind)))
                {
                    CreateApiNode(assemblyNode, semanticDocSymbol, cancellationToken);
                }
            }
        }

        private ApiNode CreateApiNode(ApiNode parentNode, ISymbol symbol, CancellationToken cancellationToken)
        {
            var symbolNamespace = symbol.ContainingNamespace.Name;
            string signature = symbol.GetSignature();
            var memberImportance = symbol.GetImportance();
            var presentedAccessibility = GetPresentedAccessibility(symbol);
            var attributes = symbol.GetAttributes().ToLookup(a => a.AttributeClass.Name, a => string.Join(", ", a.ConstructorArguments.Select(x => x.Value.ToString())));
            var apiNode = parentNode.AddMember(signature, symbolNamespace, presentedAccessibility, GetPresentedKind(symbol), symbol.Name, attributes, memberImportance);
            AddMembers(apiNode, symbol, cancellationToken);
            return apiNode;
        }

        private static SymbolKind GetPresentedKind(ISymbol symbol)
        {
            return (SymbolKind) symbol.Kind;
        }

        private static Accessibility GetPresentedAccessibility(ISymbol symbol)
        {
            var msAccessiblity = symbol.Kind == MsSymbolKind.Field && symbol.ContainingType.TypeKind == TypeKind.Enum
                ? MsAccessibility.NotApplicable
                : symbol.DeclaredAccessibility;
            return (Accessibility) msAccessiblity;
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