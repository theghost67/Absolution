using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDownsideBet : PassiveTrait
    {
        const string ID = "downside_bet";

        public tDownsideBet() : base(ID)
        {
            name = Translator.GetString("trait_downside_bet_1");
            desc = Translator.GetString("trait_downside_bet_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tDownsideBet(tDownsideBet other) : base(other) { }
        public override object Clone() => new tDownsideBet(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_downside_bet_3");
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard owner = trait.Owner;

            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(trait.GuidStr, OnCardPostKilled);
            else e.target.OnPostKilled.Remove(trait.GuidStr);
        }
        async UniTask OnCardPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard victim = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(victim.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || victim.Price < 2) return;

            await trait.AnimActivation();
            if (victim.Data.price.currency.id == "gold")
                 await trait.Side.Gold.AdjustValue(1, trait);
            else await trait.Side.Ether.AdjustValue(1, trait);
        }
    }
}
