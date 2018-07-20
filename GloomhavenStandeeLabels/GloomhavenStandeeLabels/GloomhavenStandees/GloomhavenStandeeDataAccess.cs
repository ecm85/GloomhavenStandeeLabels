using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GloomhavenStandeeLabels.GloomhavenStandees
{
    public static class GloomhavenStandeeDataAccess
    {
        public static IEnumerable<StandeeContainer> GetStandardStandeeContainers()
        {
            return GetStandeeContainers("GloomhavenStandees\\NormalStandeeData.json");
        }

        public static IEnumerable<StandeeContainer> GetBossStandeeContainers()
        {
            return GetStandeeContainers("GloomhavenStandees\\BossStandeeData.json");
        }

        private static IEnumerable<StandeeContainer> GetStandeeContainers(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<IEnumerable<StandeeContainer>>(jsonTextReader);
            }
        }
    }
}
