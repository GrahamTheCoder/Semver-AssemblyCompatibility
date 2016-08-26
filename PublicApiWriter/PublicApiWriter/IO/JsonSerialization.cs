using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyApi.ModelBuilder;
using Newtonsoft.Json;

namespace AssemblyApi.IO
{
    internal static class JsonSerialization
    {
        public static void WriteJson(IEnumerable<IApiNode> nodes, FileInfo outputFile)
        {
            var orderedNodes = nodes.OrderBy(n => n.Name).ToList();
            var serializer = new JsonSerializer() {Formatting = Formatting.None};
            using (var fileWriter = new StreamWriter(outputFile.FullName))
            using (var jsonWriter = new JsonTextWriter(fileWriter))
            {
                serializer.Serialize(jsonWriter, orderedNodes);
            }
        }

        public static IReadOnlyCollection<IApiNode> ReadJson(FileInfo outputFile)
        {
            using (var fileReader = new StreamReader(outputFile.FullName))
            using (var jsonReader = new JsonTextReader(fileReader))
            {
                return new JsonSerializer().Deserialize<ApiNode[]>(jsonReader);
            }
        }
    }
}