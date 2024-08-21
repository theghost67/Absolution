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
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tSentence(tSentence other) : base(other) { }
        public override object Clone() => new tSentence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При использовании на вражеской карте напротив</color>\nНаносит цели урон, равный количеству зарядов навыка. Тратит все заряды.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity.GetStacks(), 0);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            int damage = e.traitStacks;
            await trait.SetStacks(0, trait.Side);
            target.Drawer.CreateTextAsSpeech($"Приговор\n<size=50%>-{damage}", Color.red);
            await target.Health.AdjustValue(-damage, trait);
        }
    }
}
