using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOoo : PassiveTrait
    {
        const string ID = "ooo";
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tOoo() : base(ID)
        {
            name = "УУУрождённый";
            desc = "Эй! Знаешь что? Знаешь, приятель, я готовился, и сейчас тебе покажу!..";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tOoo(tOoo other) : base(other) { }
        public override object Clone() => new tOoo(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении карты с навыком {name} рядом с владельцем</color>\n" +
                   $"Увеличивает инциативу владельца на {_moxieF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(32, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            string entryId = trait.GuidGen(e.target.Guid);

            if (e.target.Traits.Passive(ID) == null) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await trait.Owner.Moxie.AdjustValueScale(_moxieF.Value(e.traitStacks), trait, entryId);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await trait.Owner.Moxie.RevertValueScale(entryId);
            }
        }
    }
}
