using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtc.AssemblyApi.CodeAnalysis;
using Gtc.AssemblyApi.IO;
using Gtc.AssemblyApi.ModelBuilder;

namespace Gtc.AssemblyApi
{
    internal static class Program
    {
        /// <summary>
        /// Call with an array of:
        /// Solution or project file path,
        /// Output file path to write API to,
        /// Namespace inclusion regexes (semicolon separated),
        /// Namespace exclusion regexes (semicolon separated, take precedence over inclusion)
        /// </summary>
        internal static void Main(string[] args)
        {
            var solutionOrProjectFilePath = args.ElementAtOrDefault(0);
            var humanReadableOutputFile = args.ElementAtOrDefault(1) ?? "out.txt";
            var computerReadableOutputFile = args.ElementAtOrDefault(2) ?? "out.json";
            var includeRegexes = (args.ElementAtOrDefault(3) ?? "");
            var excludeRegexes = (args.ElementAtOrDefault(4) ?? "");

            if (string.IsNullOrEmpty(solutionOrProjectFilePath) || !File.Exists(solutionOrProjectFilePath))
            {
                PrintUsage();
                return;
            }

            var printerConfig = new PrinterConfig(includeRegexes, excludeRegexes);
            WritePublicApi(new PublicApiWriter(), printerConfig, solutionOrProjectFilePath, humanReadableOutputFile, computerReadableOutputFile, new CancellationTokenSource().Token).Wait();
        }

        private static async Task WritePublicApi(PublicApiWriter publicApiWriter, PrinterConfig printerConfig, string solutionOrProjectFilePath, string humanReadableFileName, string jsonFileName, CancellationToken cancellationToken)
        {
            var solutionNodes = await ApiReader.ReadApiFromProjects(solutionOrProjectFilePath, cancellationToken);
            var filteredNodes = FilteredApiNode.For(printerConfig, solutionNodes).ToList();
            var writeHumanReadable = publicApiWriter.WriteHumanReadable(filteredNodes, new FileInfo(humanReadableFileName), cancellationToken);
            JsonSerialization.WriteJson(filteredNodes, new FileInfo(jsonFileName));
            await writeHumanReadable;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe [solution or project path] [output file path] [include regexes] [exclude regexes]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln ");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln out.txt MyApp;MyLibrary MyApp.Tests;MyLibrary.Tests");
        }
    }
}