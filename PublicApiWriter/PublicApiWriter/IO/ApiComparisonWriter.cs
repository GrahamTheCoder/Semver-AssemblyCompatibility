using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtc.AssemblyApi.Comparison;

namespace Gtc.AssemblyApi.IO
{
    internal class ApiComparisonWriter
    {
        private readonly ApiNodeWriter m_ApiNodeWriter;

        public ApiComparisonWriter(ApiNodeWriter apiNodeWriter)
        {
            m_ApiNodeWriter = apiNodeWriter;
        }

        public static async Task Write(IEnumerable<IApiNodeComparison> members, StringWriter stringWriter)
        {
            var writer = new ApiComparisonWriter(new ApiNodeWriter());
            foreach (var apiNodeComparison in members)
            {
                await writer.Write(apiNodeComparison, stringWriter, CancellationToken.None);
            }
        }

        public async Task Write(IApiNodeComparison apiComparison, TextWriter file, CancellationToken cancellationToken, int indentLevel = 0)
        {
            await WriteLineStart(apiComparison, file, indentLevel);
            await m_ApiNodeWriter.Write(apiComparison.Get(n => n), file, cancellationToken, false);
            var orderedMembers = MembersInCanonicalOrder(apiComparison);
            foreach (var member in orderedMembers)
            {
                await Write(member, file, cancellationToken, indentLevel + 1);
            }
        }

        private async Task WriteLineStart(IApiNodeComparison apiComparison, TextWriter file, int indentLevel)
        {
            file.WriteLine();
            await file.WriteAsync(ChangeTypeIndicator(apiComparison) + new string(' ', indentLevel * 2 + 1));
        }

        private static IOrderedEnumerable<IApiNodeComparison> MembersInCanonicalOrder(IApiNodeComparison apiNode)
        {
            return apiNode.MemberComparison
                .OrderByDescending(m => m.Get(n => n), new PrettyNodeOrderComparer());
        }


        private string ChangeTypeIndicator(IApiNodeComparison nodeComparison)
        {
            switch (nodeComparison.SignatureDifferenceType)
            {
                case SignatureDifferenceType.SignatureSame:
                    return " ";
                case SignatureDifferenceType.Added:
                    return "+";
                case SignatureDifferenceType.Removed:
                    return "-";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
