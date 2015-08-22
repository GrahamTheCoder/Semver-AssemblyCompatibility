using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace PublicApiWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionFilePath = args.ElementAtOrDefault(0);
            var outputFile = args.ElementAtOrDefault(1) ?? "out.txt";
            var namespacePrefixIncludes = (args.ElementAtOrDefault(2) ?? "");
            var namespacePrefixExcludes = (args.ElementAtOrDefault(3) ?? "");
            var printerConfig = new PrinterConfig(namespacePrefixIncludes, namespacePrefixExcludes);
            var cancellationToken = new CancellationTokenSource().Token;
            if (string.IsNullOrEmpty(solutionFilePath) || !File.Exists(solutionFilePath))
            {
                PrintUsage();
            }

            var msWorkspace = MSBuildWorkspace.Create();
            var solution = new ApiReader(msWorkspace.OpenSolutionAsync(solutionFilePath, cancellationToken).Result);
            solution.WritePublicMembers(outputFile, printerConfig, cancellationToken).Wait(cancellationToken);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe solutionpath [output file path] [namespace prefix includes] [namespace prefix excludes]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln ");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln out.txt MyApp;MyLibrary MyApp.Tests;MyLibrary.Tests");
        }
    }

    internal class PrinterConfig
    {
        public string[] NamespacePrefixIncludes { get; }
        public string[] NamespacePrefixExcludes { get; }
        public Accessibility Accessibility { get; } = Accessibility.Public;

        public PrinterConfig(string semiColonSeparatedPrefixIncludes, string semiColonSeparatedPrefixExcludes)
        {
            var splitters = new[]{ ";" };
            NamespacePrefixIncludes = semiColonSeparatedPrefixIncludes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            NamespacePrefixExcludes = semiColonSeparatedPrefixExcludes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        }

        public void set_Value(bool value)
        {

        }
        public bool ShouldPrint(string @namespace, Accessibility symbolAccessibility)
        {
            return symbolAccessibility >= Accessibility
                   && IsIncluded(@namespace)
                   && !IsExcluded(@namespace);
        }

        private bool IsIncluded(string ns)
        {
            return !NamespacePrefixIncludes.Any() || NamespacePrefixIncludes.Any(p => ns.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
        private bool IsExcluded(string ns)
        {
            return NamespacePrefixExcludes.Any(p => ns.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}