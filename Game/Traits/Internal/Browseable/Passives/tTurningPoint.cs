using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTurningPoint : PassiveTrait
    {
        const string ID = "turning_point";

        public tTurningPoint() : base(ID)
        {
            name = Translator.GetString("trait_turning_point_1");
            desc = Translator.GetString("trait_turning_point_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tTurningPoint(tTurningPoint other) : base(other) { }
        public override object Clone() => new tTurningPoint(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_turning_point_3");

        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (!e.canSeeTarget) return;

            BattleFieldCard owner = e.trait.Owner;
            if (e.target.Moxie >= owner.Moxie) return;

            await e.trait.AnimDetectionOnSeen(e.target);
            await e.target.Health.AdjustValue(-owner.Strength, e.trait);
        }
    }
}
