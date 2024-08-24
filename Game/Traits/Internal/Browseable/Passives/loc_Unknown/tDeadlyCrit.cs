using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDeadlyCrit : PassiveTrait
    {
        const string ID = "deadly_crit";
        const int PRIORITY = 5;
        const int ATTACKS_NEEDED = 3;
        static readonly TraitStatFormula _strengthF = new(true, 0.50f, 0.50f);

        public tDeadlyCrit() : base(ID)
        {
            name = "Смертельный крит";
            desc = "Давай, бей Фантомку.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tDeadlyCrit(tDeadlyCrit other) : base(other) { }
        public override object Clone() => new tDeadlyCrit(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При каждой {ATTACKS_NEEDED}-й атаке владельца (П{PRIORITY})</color>\n" +
                   $"увеличивает силу атаки на {_strengthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(16, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            int attacksCount = trait.Storage.ContainsKey(ID) ? (int)trait.Storage[ID] + 1 : 1;
            if (attacksCount <= ATTACKS_NEEDED)
            {
                trait.Storage[ID] = attacksCount;
                return;
            }

            trait.Storage[ID] = 0;
            await trait.AnimActivation();
            await e.Strength.AdjustValueScale(_strengthF.Value(trait.GetStacks()), trait);
        }
    }
}
