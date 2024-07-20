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
    public class tSummarizing : ActiveTrait
    {
        const string ID = "summarizing";
        const int PRIORITY = 5;
        const float DAMAGE_RECEIVED_RATIO = 1.00f;

        public tSummarizing() : base(ID)
        {
            name = "Резюмирование";
            desc = "";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tSummarizing(tSummarizing other) : base(other) { }
        public override object Clone() => new tSummarizing(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = DAMAGE_RECEIVED_RATIO * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После получения атакующей инициации владельцем (П{PRIORITY})",
                    $"Увеличивает количество зарядов данного навыка на {effect}% от полученного урона."),
                new($"При использовании на карте напротив, если зарядов > 1",
                    $"Наносит цели урон, равный количеству зарядов навыка. Сбрасывает заряды до 1."),
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

            BattleActiveTrait trait = (BattleActiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            BattleActiveTrait trait = (BattleActiveTrait)e.trait;

            int damage = e.trait.GetStacks();
            target.Drawer.CreateTextAsSpeech($"Итог\n<size=50%>-{damage}", Color.red);
            await target.health.AdjustValue(-damage, trait);
            await trait.SetStacks(1, trait.Side);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs rArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            int stacks = (rArgs.strength * DAMAGE_RECEIVED_RATIO).Ceiling();
            await trait.AdjustStacks(stacks, trait);
        }
    }
}
