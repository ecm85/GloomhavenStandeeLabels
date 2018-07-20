using System.Collections.Generic;

namespace Avery16282Generator.Dominion
{
    public class CardSet
    {
        public IEnumerable<string> Edition { get; set; }
        public string Image { get; set; }
        public string Set_name { get; set; }
        public string Set_text { get; set; }
        public string Text_icon { get; set; }
    }
}
