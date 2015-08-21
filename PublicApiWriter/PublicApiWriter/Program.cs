using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.MSBuild;

namespace PublicApiWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionFilePath = args.ElementAtOrDefault(0);
            var outputFile = args.ElementAtOrDefault(1) ?? "out.txt";
            var cancellationToken = new CancellationTokenSource().Token;
            if (string.IsNullOrEmpty(solutionFilePath) || !File.Exists(solutionFilePath))
            {
                PrintUsage();
            }

            var msWorkspace = MSBuildWorkspace.Create();
            var solution = new ApiReader(msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken).Result);
            solution.WritePublicMembers(outputFile, cancellationToken).Wait(cancellationToken);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe solutionpath [output file path]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln");
        }
    }
}