using Game.Cards;
using System;
namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для динамического описания карты (см. <see cref="Card"/>, <see cref="IDescriptive"/>).
    /// </summary>
    public class CardDescriptiveArgs : DescriptiveArgs
    {
        // length: 4 // [0] = price, [1] = moxie, [2] = health, [3] = strength
        public static readonly int[] normalStats = new int[] { 0, 0, 1, 0 };
        public readonly new Card src;
        public int[] linkStats;
        public TraitStacksPair[] linkTraits;

        public CardDescriptiveArgs(string id) : base(CardBrowser.GetCard(id))
        {
            src = (Card)base.src;
            linkStats = new int[] { -1, -1, -1, -1 };
            linkTraits = Array.Empty<TraitStacksPair>();
        }
    }
}
