using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDeadlyCrit : PassiveTrait
    {
        const string ID = "deadly_crit";
        const int PRIORITY = 5;
        const float DAMAGE_REL_INCREASE = 1.00f;
        const int ATTACKS_NEEDED = 3;

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

        public override string DescRich(ITableTrait trait)
        {
            float strengthRel = DAMAGE_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При каждой {ATTACKS_NEEDED}-й атаке владельца (П{PRIORITY})",
                    $"увеличивает силу атаки на <u>{strengthRel}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(60, stacks);
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
            await e.strength.AdjustValueScale(DAMAGE_REL_INCREASE * trait.GetStacks(), trait);
        }
    }
}
