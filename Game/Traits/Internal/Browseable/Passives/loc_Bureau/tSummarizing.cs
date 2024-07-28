using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSummarizing : PassiveTrait
    {
        const string ID = "summarizing";
        const int PRIORITY = 5;
        const string TRAIT_ID = "sentence";
        const float DAMAGE_RECEIVED_RATIO_BASE = 1.00f;
        const float DAMAGE_RECEIVED_RATIO_PER_STACK = 0.50f;

        public tSummarizing() : base(ID)
        {
            name = "Резюмирование";
            desc = "Приговор может быть смертельнее всякого оружия.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tSummarizing(tSummarizing other) : base(other) { }
        public override object Clone() => new tSummarizing(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            float effect = (DAMAGE_RECEIVED_RATIO_BASE + DAMAGE_RECEIVED_RATIO_PER_STACK * trait.GetStacks()) * 100;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После атаки на владельца (П{PRIORITY})",
                    $"Увеличивает количество зарядов навыка <i>{traitName}</i> на {effect}% от силы атаки."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 40 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            float ratio = DAMAGE_RECEIVED_RATIO_BASE + DAMAGE_RECEIVED_RATIO_PER_STACK * trait.GetStacks();
            int stacks = (e.strength * ratio).Ceiling();
            await owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
