using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Avery16282Generator.Dominion
{
    public static class DominionCardDataAccess
    {
        public static IEnumerable<DominionCard> GetCardsToPrint()
        {
            var cardTypes = GetCardTypes().ToList();
            var cardSets = GetCardSets();
            var cards = GetCards(cardSets, cardTypes);

            var setsToPrint = cardSets.Values.Select(set => set.Set_name).ToList();
            //if (Settings.Default.UseBlackList)
            //{
            //    setsToPrint = setsToPrint.Where(setToPrint => !Settings.Default.BlackList.Contains(setToPrint)).ToList();
            //}
            //if (Settings.Default.UseWhiteList)
            //{
            //    setsToPrint = setsToPrint.Where(setToPrint => Settings.Default.WhiteList.Contains(setToPrint)).ToList();
            //}
            //if (Settings.Default.UseBannedKeywords)
            //{
            //    setsToPrint = setsToPrint
            //        .Where(
            //            setToPrint => Settings.Default.BannedKeywords
            //                .Cast<string>()
            //                .All(bannedKeyword => !setToPrint.Contains(bannedKeyword)))
            //        .ToList();
            //}

            var cardFromSetsToPrint = cards
                .Where(card => setsToPrint.Contains(card.Set.Set_name))
                .ToList();

            var groupedCards = cardFromSetsToPrint.Where(card => !string.IsNullOrWhiteSpace(card.Group_tag)).ToList();
            var nonGroupedCards = cardFromSetsToPrint.Except(groupedCards);
            var groupedCardsToPrint = groupedCards.GroupBy(card => card.Group_tag)
                .Select(cardGroup => cardGroup.SingleOrDefault(card => card.Group_top) ?? cardGroup.First())
                .ToList();
            var cardsToPrint = nonGroupedCards.Concat(groupedCardsToPrint).ToList();
            return cardsToPrint;
        }

        private static IEnumerable<DominionCard> GetCards(IDictionary<string, CardSet> cardSets, IList<CardSuperType> cardSuperTypes)
        {
            IEnumerable<DominionCard> cards;
            using (var fileStream = new FileStream("Dominion\\cards_db.json", FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                cards = serializer.Deserialize<IEnumerable<DominionCard>>(jsonTextReader).ToList();
            }

            IDictionary<string, DominionCard> englishCards;
            using (var fileStream = new FileStream("Dominion\\cards_en_us.json", FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                englishCards = serializer.Deserialize<IDictionary<string, DominionCard>>(jsonTextReader);
            }

            return cardSets.Keys
                .SelectMany(key =>
                    cards
                        .Where(card => card.Cardset_tags.Contains(key))
                        .Select(card => new DominionCard
                        {
                            Group_tag = card.Group_tag,
                            Types = card.Types,
                            Name = englishCards[card.Card_tag].Name,
                            Card_tag = card.Card_tag,
                            Cardset_tags = card.Cardset_tags,
                            GroupName = string.IsNullOrWhiteSpace(card.Group_tag) ? null : englishCards[card.Group_tag].Name,
                            Group_top = card.Group_top,
                            Cost = card.Cost,
                            Debtcost = card.Debtcost,
                            Potcost = card.Potcost,
                            Set = cardSets[key],
                            SuperType = cardSuperTypes.Single(cardSuperType => cardSuperType.Card_type.OrderBy(i => i).SequenceEqual(card.Types.OrderBy(i => i)))
                        })
                        .ToList());
        }

        private static IDictionary<string, CardSet> GetCardSets()
        {
            IDictionary<string, CardSet> cardSets;
            using (var fileStream = new FileStream("Dominion\\sets_db.json", FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                cardSets = serializer.Deserialize<Dictionary<string, CardSet>>(jsonTextReader);
            }

            IDictionary<string, CardSet> englishCardSets;
            using (var fileStream = new FileStream("Dominion\\sets_en_us.json", FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                englishCardSets = serializer.Deserialize<Dictionary<string, CardSet>>(jsonTextReader);
            }

            foreach (var cardSetName in cardSets.Keys)
            {
                cardSets[cardSetName].Set_name = englishCardSets[cardSetName].Set_name;
                cardSets[cardSetName].Set_text = englishCardSets[cardSetName].Set_text;
                cardSets[cardSetName].Text_icon = englishCardSets[cardSetName].Text_icon;
            }
            return cardSets;
        }

        private static IEnumerable<CardSuperType> GetCardTypes()
        {
            using (var fileStream = new FileStream("Dominion\\types_db.json", FileMode.Open))
            using (var reader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<IEnumerable<CardSuperType>>(jsonTextReader);
            }
        }
    }
}
