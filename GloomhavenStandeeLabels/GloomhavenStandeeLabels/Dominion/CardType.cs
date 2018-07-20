using System.Collections.Generic;

namespace Avery16282Generator.Dominion
{
    public class CardSuperType
    {
        public IEnumerable<string> Card_type { get; set; }
        public string Card_type_image { get; set; }
        public int DefaultCardCount { get; set; }
    }
}
