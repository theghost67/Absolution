using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTriptocainum : ActiveTrait
    {
        const string ID = "triptocainum";
        const int PRIORITY = 7;
        const int CD = 3;
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.50f);

        public tTriptocainum() : base(ID)
        {
            name = "Триптокаинум";
            desc = "КОКАИНУМ!";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tTriptocainum(tTriptocainum other) : base(other) { }
        public override object Clone() => new tTriptocainum(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании",
                    $"Увеличивает силу владельца на {_strengthF.Format(trait)}. После каждой последующей атаки владельца (П{PRIORITY}), он окажется на пороге смерти (игнор. здоровья). Перезарядка: {CD} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(80, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.1f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            IBattleTrait trait = (IBattleTrait)e.trait;

            float strength = _strengthF.Value(trait);
            await trait.Owner.Strength.AdjustValueScale(strength, trait);
            trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnInitiationPostSent);
            trait.Storage.turnsDelay += CD;
        }

        static async UniTask OnInitiationPostSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await owner.TryKill(BattleKillMode.IgnoreHealthRestore, trait);
        }
    }
}
