using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSunrisingFlame : ActiveTrait
    {
        const string ID = "sunrising_flame";
        const string TRAIT_ID = "flame";

        public tSunrisingFlame() : base(ID)
        {
            name = "Пламя восходящего солнца";
            desc = "Ещё один шаг, и я сожгу тебя заживо.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tSunrisingFlame(tSunrisingFlame other) : base(other) { }
        public override object Clone() => new tSunrisingFlame(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на территории</color>\nУбивает владельца (игнор. восстановления и неуязвимости), " +
                   $"равномерно накладывая навык {traitName} на всех противников. " +
                   $"Общее количество накладываемых зарядов равняется здоровью владельца, делённому на количество противников.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            BattleField ownerField = owner.Field;
            int health = owner.Health;

            await owner.TryKill(BattleKillMode.IgnoreEverything, trait);

            BattleFieldCard[] cards = owner.Territory.Fields(ownerField.pos, TerritoryRange.oppositeAll).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;
            int stacks = (int)Mathf.Ceil((float)health / cards.Length);

            foreach (BattleFieldCard card in cards)
                await card.Traits.Passives.AdjustStacks(TRAIT_ID, stacks, null);
        }
    }
}
