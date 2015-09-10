using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            if (string.IsNullOrEmpty(solutionFilePath) || !File.Exists(solutionFilePath))
            {
                PrintUsage();
                return;
            }

            var publicApiWriter = CreateWriter(includeRegexes, excludeRegexes);
            WritePublicApi(publicApiWriter, solutionFilePath, outputFile, new CancellationTokenSource().Token).Wait();
        }

        private static PublicApiWriter CreateWriter(string includeRegexes, string excludeRegexes)
        {
            return new PublicApiWriter(new PrinterConfig(includeRegexes, excludeRegexes));
        }

        private static async Task WritePublicApi(PublicApiWriter publicApiWriter, string solutionFilePath, string outputFile, CancellationToken cancellationToken)
        {
            var solutionNode = await ApiReader.ReadApiFromSolution(solutionFilePath, cancellationToken);
            await publicApiWriter.Write(solutionNode, outputFile, cancellationToken);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PublicApiWriter.exe solutionpath [output file path] [include regexes] [exclude regexes]");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln ");
            Console.WriteLine("Example: PublicApiWriter.exe mySolution.sln out.txt MyApp;MyLibrary MyApp.Tests;MyLibrary.Tests");
        }
    }
}