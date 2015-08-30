using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text.RegularExpressions;

namespace PublicApiWriter
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

    internal class PrinterConfig
    {
        public string[] IncludeSignatureRegexes { get; }
        public string[] ExcludeSignatureRegexes { get; }
        public Accessibility Accessibility { get; } = Accessibility.Public;

        public PrinterConfig(string semiColonSeparatedIncludeRegexes, string semiColonSeparatedExcludeRegexes)
        {
            var splitters = new[]{ ";" };
            IncludeSignatureRegexes = semiColonSeparatedIncludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            ExcludeSignatureRegexes = semiColonSeparatedExcludeRegexes.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool ShouldPrint(string @namespace, Accessibility symbolAccessibility)
        {
            return symbolAccessibility >= Accessibility
                   && IsIncluded(@namespace)
                   && !IsExcluded(@namespace);
        }

        private bool IsIncluded(string ns)
        {
            return !IncludeSignatureRegexes.Any() || IncludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
        }

        private bool IsExcluded(string ns)
        {
            return ExcludeSignatureRegexes.Any(p => Regex.IsMatch(ns, p, RegexOptions.IgnoreCase));
        }
    }
}