using Game.Cards;
using Game.Traits;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Класс, представляющий улилиты динамического описания (см. <see cref="IDescriptive"/>).
    /// </summary>
    public static class DescriptiveUtils
    {
        public static string DescDynamic(this ITableCard card, out CardDescriptiveArgs args)
        {
            args = CardArgsCreator(card, out _);
            return card.Data.DescDynamic(args);
        }
        public static string DescDynamic(this ITableTrait trait, out TraitDescriptiveArgs args)
        {
            args = TraitArgsCreator(trait, out _);
            return trait.Data.DescDynamic(args);
        }

        public static string DescDynamicWithLinks(this ITableCard card, out string[] linksTexts)
        {
            CardDescriptiveArgs args = CardArgsCreator(card, out DescLinkCollection links);
            linksTexts = DescLinksTextsRecursive(links, new List<DescriptiveArgs>());
            return card.Data.DescDynamic(args);
        }
        public static string DescDynamicWithLinks(this ITableTrait trait, out string[] linksTexts)
        {
            TraitDescriptiveArgs args = TraitArgsCreator(trait, out DescLinkCollection links);
            linksTexts = DescLinksTextsRecursive(links, new List<DescriptiveArgs>());
            return trait.Data.DescDynamic(args);
        }

        static string[] DescLinksTextsRecursive(DescLinkCollection descLinks, List<DescriptiveArgs> usedLinks)
        {
            if (descLinks.Count == 0) return System.Array.Empty<string>();
            List<string> list = new(descLinks.Count);
            foreach (DescriptiveArgs linkArgs in descLinks)
            {
                // do not add same link (even if it has different stats/traits for card) - use 'linkFlag = false' or explicit description in this case
                if (usedLinks.Contains(linkArgs)) continue;

                if (linkArgs.isCard)
                {
                    Card card = (Card)linkArgs.data;
                    CardDescriptiveArgs args = (CardDescriptiveArgs)linkArgs;
                    list.Add(card.DescDynamic(args));
                    list.AddRange(DescLinksTextsRecursive(card.DescLinks(args), usedLinks));
                }
                else
                {
                    Trait trait = (Trait)linkArgs.data;
                    TraitDescriptiveArgs args = (TraitDescriptiveArgs)linkArgs;
                    list.Add(trait.DescDynamic(args));
                    list.AddRange(DescLinksTextsRecursive(trait.DescLinks(args), usedLinks));
                }
            }
            return list.ToArray();
        }
        static CardDescriptiveArgs CardArgsCreator(ITableCard card, out DescLinkCollection links)
        {
            CardDescriptiveArgs args = new(card);
            links = args.data.DescLinks(args);
            args.linkFormat = false;
            args.linksAreAvailable = links.Count > 0;
            return args;
        }
        static TraitDescriptiveArgs TraitArgsCreator(ITableTrait trait, out DescLinkCollection links)
        {
            TraitDescriptiveArgs args = new(trait);
            args.linkFormat = false;
            args.turnsDelay = trait.TurnDelay;
            args.stacks = trait.GetStacks();
            links = args.data.DescLinks(args);
            args.linksAreAvailable = links.Count > 0;
            return args;
        }
    }
}
