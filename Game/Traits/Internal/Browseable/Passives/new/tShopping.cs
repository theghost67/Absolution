using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tShopping : PassiveTrait
    {
        const string ID = "shopping";

        public tShopping() : base(ID)
        {
            name = "Шоппинг";
            desc = "Не забудь, кто тебя выручил.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tShopping(tShopping other) : base(other) { }
        public override object Clone() => new tShopping(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После смерти любой вражеской карты</color>\nСторона-владелец получает 1 ед. золота.";
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(e.trait.GuidStr, OnTargetPostKilled);
            else e.target.OnPostKilled.Remove(e.trait.GuidStr);
        }
        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(target.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.IsKilled || owner.Field == null) return;

            await trait.AnimActivation();
            await trait.Side.Gold.AdjustValue(1, trait);
        }
    }
}
