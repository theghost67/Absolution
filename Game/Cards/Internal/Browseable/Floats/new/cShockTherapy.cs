using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System.Linq;

namespace Game.Cards
{
    public class cShockTherapy : FloatCard
    {
        const string ID = "shock_therapy";
        const string TRAIT_ID = "shock";

        public cShockTherapy() : base(ID)
        {
            name = "Электрошоковая терапия";
            desc = "Внимание: электрошоковая терапия может привести к необратимым неврологическим изменениям, " +
                   "просьба принимать решение о назначении данной процедуры только в крайнем случае. Комментарии врача: Ы-ХЫ-ХЫ-ХЫ-ХЫ-ХЫ!";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cShockTherapy(cShockTherapy other) : base(other) { }
        public override object Clone() => new cShockTherapy(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"Даёт всем вражеским картам навык <nobr><u>{traitName}</u></nobr>.";
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            BattleFieldCard[] cards = card.Side.Opposite.Fields().WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard c in cards)
                await c.Traits.Passives.AdjustStacks(TRAIT_ID, 1, card);
        }
    }
}
