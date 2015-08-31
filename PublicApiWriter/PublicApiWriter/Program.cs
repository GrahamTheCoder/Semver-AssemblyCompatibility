using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.MSBuild;
using AssemblyApi.Output;

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
            var printerConfig = new PrinterConfig(includeRegexes, excludeRegexes);
            var cancellationToken = new CancellationTokenSource().Token;
            if (string.IsNullOrEmpty(solutionFilePath) || !File.Exists(solutionFilePath))
            {
                PrintUsage();
            }

            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                var result = msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken).Result;
                var solution = new ApiReader(result);
                var solutionNode = solution.ReadProjects(cancellationToken);
                new PublicApiWriter(printerConfig).Write(solutionNode, outputFile, cancellationToken).Wait(cancellationToken);
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