using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSadNews : PassiveTrait
    {
        const string ID = "sad_news";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tSadNews() : base(ID)
        {
            name = "Печальные вести";
            desc = "Вынужден вас расстроить - вы скоро нас покинете.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tSadNews(tSadNews other) : base(other) { }
        public override object Clone() => new tSadNews(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Будучи на территории, после смерти любой карты</color>\nПонижает инициативу владельца на {_moxieF.Format(args.stacks)} " +
                   $"и повышает его силу на {_strengthF.Format(args.stacks)}";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
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

            int stacks = trait.GetStacks();
            int moxie = -_moxieF.ValueInt(stacks);
            int strength = _strengthF.ValueInt(stacks);
            await trait.AnimActivation();
            await owner.Moxie.AdjustValue(moxie, trait);
            await owner.Strength.AdjustValue(strength, trait);
        }
    }
}
