using System.Threading;
using Newtonsoft.Json;

namespace GloomhavenStandeeLabels.GloomhavenStandees
{
    public class Standee
    {
        [JsonProperty]
        private string Name { get; set; }
        public int StandeeCount { get; set; }

        public string DisplayName => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Name.ToLower());
    }
}
