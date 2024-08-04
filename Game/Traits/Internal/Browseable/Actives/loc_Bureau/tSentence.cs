using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSentence : ActiveTrait
    {
        const string ID = "sentence";

        public tSentence() : base(ID)
        {
            name = "Приговор";
            desc = "Вынесение приговора!";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tSentence(tSentence other) : base(other) { }
        public override object Clone() => new tSentence(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"Наносит цели урон, равный количеству зарядов навыка. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 40 * Mathf.Pow(stacks - 1, 2);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity.GetStacks(), 0);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Opposite.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Opposite.Card;

            int damage = e.trait.GetStacks();
            target.Drawer.CreateTextAsSpeech($"Приговор\n<size=50%>-{damage}", Color.red);
            await target.health.AdjustValue(-damage, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
