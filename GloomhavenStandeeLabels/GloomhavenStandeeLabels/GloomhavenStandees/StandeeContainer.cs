using System.Collections.Generic;

namespace GloomhavenStandeeLabels.GloomhavenStandees
{
    public class StandeeContainer
    {
        public IEnumerable<string> Description { get; set; }
        public IEnumerable<StandeeGroup> StandeeGroups { get; set; }
    }
}
