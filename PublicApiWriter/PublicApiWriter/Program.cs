using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssemblyApi.ModelBuilder;
using AssemblyApi.Output;
using Newtonsoft.Json;

namespace AssemblyApi
{
    internal static class Program
    {
        /// <summary>
        /// Call will an array of:
        /// Solution or project file path,
        /// Output file path to write API to,
        /// Namespace inclusion regexes (semicolon separated),
        /// Namespace exclusion regexes (semicolon separated, take precedence over inclusion)
        /// </summary>
        internal static void Main(string[] args)
        {
            var solutionOrProjectFilePath = args.ElementAtOrDefault(0);
            var outputFile = args.ElementAtOrDefault(1) ?? "out.txt";
            var includeRegexes = (args.ElementAtOrDefault(2) ?? "");
            var excludeRegexes = (args.ElementAtOrDefault(3) ?? "");

            if (string.IsNullOrEmpty(solutionOrProjectFilePath) || !File.Exists(solutionOrProjectFilePath))
            {
                PrintUsage();
                return;
            }

            var printerConfig = new PrinterConfig(includeRegexes, excludeRegexes);
            WritePublicApi(new PublicApiWriter(), printerConfig, solutionOrProjectFilePath, outputFile, new CancellationTokenSource().Token).Wait();
        }

        private static async Task WritePublicApi(PublicApiWriter publicApiWriter, PrinterConfig printerConfig, string solutionOrProjectFilePath, string outputFile, CancellationToken cancellationToken)
        {
            var solutionNodes = await ApiReader.ReadApiFromProjects(solutionOrProjectFilePath, cancellationToken);
            new ApiFilter(printerConfig).ApplyTo(solutionNodes);
            await publicApiWriter.WriteHumanReadable(solutionNodes, outputFile, cancellationToken);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe [solution or project path] [output file path] [include regexes] [exclude regexes]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln ");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln out.txt MyApp;MyLibrary MyApp.Tests;MyLibrary.Tests");
        }
    }
}