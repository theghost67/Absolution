using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOnLookout : PassiveTrait
    {
        const string ID = "on_lookout";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tOnLookout() : base(ID)
        {
            name = "На стрёме";
            desc = "Давай, проходи, я прикрою.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle, PRIORITY);
        }
        protected tOnLookout(tOnLookout other) : base(other) { }
        public override object Clone() => new tOnLookout(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении карты напротив владельца (П{PRIORITY})</color>\n" +
                   $"Мгновенно атакует цель с силой в {_strengthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(6, stacks);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            if (!e.canSeeTarget) return;

            await trait.AnimDetectionOnSeen(e.target);
            int strength = _strengthF.ValueInt(e.traitStacks);
            BattleInitiationSendArgs initiation = new(trait.Owner, strength, true, false, e.target.Field);
            trait.Territory.Initiations.EnqueueAndRun(initiation);
        }        
    }
}
