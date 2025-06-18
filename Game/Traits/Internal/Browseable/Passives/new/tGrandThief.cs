using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tGrandThief : PassiveTrait
    {
        const string ID = "grand_thief";
        static readonly TraitStatFormula _priceF = new(false, 1, 0);

        public tGrandThief() : base(ID)
        {
            name = Translator.GetString("trait_grand_thief_1");
            desc = Translator.GetString("trait_grand_thief_2");

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tGrandThief(tGrandThief other) : base(other) { }
        public override object Clone() => new tGrandThief(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_grand_thief_3", _priceF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerPreKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerPreKilled(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Side.Opposite.Fields().WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int price = _priceF.ValueInt(trait.GetStacks());
            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.Price.AdjustValue(-price, trait);
        }
    }
}
