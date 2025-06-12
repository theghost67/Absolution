using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Palette;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCreatorsMark : PassiveTrait
    {
        const string ID = "creators_mark";

        public tCreatorsMark() : base(ID)
        {
            name = "Метка создателя";
            desc = $"<color={ColorPalette.CP.Hex}>null</color>";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tCreatorsMark(tCreatorsMark other) : base(other) { }
        public override object Clone() => new tCreatorsMark(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"Интересно, что произойдёт, если разместить пять уникальных карт с таким навыком на территории? Хм, интересно...";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Side.Fields().WithCard().Select(f => f.Card).ToArray();
            if (cards.Length != BattleTerritory.MAX_WIDTH) return;

            HashSet<string> cardsIds = new(BattleTerritory.MAX_WIDTH);
            foreach (BattleFieldCard card in cards)
                cardsIds.Add(card.Data.id);
            if (cardsIds.Count != BattleTerritory.MAX_WIDTH) return;

            await trait.AnimActivation();
            await trait.Side.Gold.AdjustValue(100, trait);
            await trait.Side.Ether.AdjustValue(100, trait);
            foreach (BattleFieldCard card in cards)
                await card.Traits.Passives.SetStacks(ID, 0, trait);
        }
    }
}
