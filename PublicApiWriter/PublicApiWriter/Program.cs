using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssemblyApi.Output;
using Microsoft.CodeAnalysis.MSBuild;

namespace AssemblyApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionFilePath = args.ElementAtOrDefault(0);
            var outputFile = args.ElementAtOrDefault(1) ?? "out.txt";
            var includeRegexes = (args.ElementAtOrDefault(2) ?? "");
            var excludeRegexes = (args.ElementAtOrDefault(3) ?? "");

            if (string.IsNullOrEmpty(solutionFilePath) || !File.Exists(solutionFilePath))
            {
                PrintUsage();
                return;
            }

            var publicApiWriter = new PublicApiWriter(new PrinterConfig(includeRegexes, excludeRegexes));
            WritePublicApi(publicApiWriter, solutionFilePath, outputFile, new CancellationTokenSource().Token).Wait();
        }

        private static async Task WritePublicApi(PublicApiWriter publicApiWriter, string solutionFilePath, string outputFile, CancellationToken cancellationToken)
        {
            var solutionNode = await ReadApiFromSolution(solutionFilePath, cancellationToken);
            await publicApiWriter.Write(solutionNode, outputFile, cancellationToken);
        }

        private static async Task<IEnumerable<ApiNode>> ReadApiFromSolution(string solutionFilePath, CancellationToken cancellationToken)
        {
            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                var result = await msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken);
                var solution = new ApiReader(result);
                return await solution.ReadProjects(cancellationToken);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe solutionpath [output file path] [include regexes] [exclude regexes]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln ");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln out.txt MyApp;MyLibrary MyApp.Tests;MyLibrary.Tests");
        }
    }
}