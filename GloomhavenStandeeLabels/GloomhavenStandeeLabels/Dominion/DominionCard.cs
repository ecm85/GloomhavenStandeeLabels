using System.Collections.Generic;

namespace Avery16282Generator.Dominion
{
    public class DominionCard
    {
        public string Card_tag { get; set; }
        public IEnumerable<string> Cardset_tags { get; set; }
        public string Cost { get; set; }
        public int? Debtcost { get; set; }
        public string Group_tag { get; set; }
        public bool Group_top { get; set; }
        public IEnumerable<string> Types { get; set; }
        public string Name { get; set; }
        public int? Potcost { get; set; }
        public CardSet Set { get; set; }
        public CardSuperType SuperType { get; set; }
        public string GroupName { get; set; }
    }
}
