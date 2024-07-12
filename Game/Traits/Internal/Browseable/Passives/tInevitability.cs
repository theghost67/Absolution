using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tInevitability : PassiveTrait
    {
        const string ID = "inevitability";
        const int PRIORITY = 5;
        const float HEALTH_REL_INCREASE = 0.25f;

        public tInevitability() : base(ID)
        {
            name = "Неотвратимость";
            desc = "Я - сама неотвратимость.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tInevitability(tInevitability other) : base(other) { }
        public override object Clone() => new tInevitability(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = HEALTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти любой карты на территории, кроме себя (П{PRIORITY}/Т)",
                    $"увеличивает здоровье владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 12 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;
            if (trait.WasAdded(e))
            {
                foreach (BattleField field in trait.Territory.Fields())
                {
                    field.Card?.OnPostKilled.Add(trait.GuidStrForEvents(0), OnTerritoryCardPostKilled, PRIORITY);
                    field.OnCardAttached.Add(trait.GuidStrForEvents(0), OnCardAttachedToAnyField, PRIORITY);
                }
            }
            else if (trait.WasRemoved(e))
            {
                foreach (BattleField field in trait.Territory.Fields())
                {
                    field.Card?.OnPostKilled.Remove(trait.GuidStrForEvents(0));
                    field.OnCardAttached.Remove(trait.GuidStrForEvents(0));
                }
            }
        }

        async UniTask OnCardAttachedToAnyField(object sender, EventArgs e)
        {
            BattleField field = (BattleField)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(field.Territory);
            field.Card.OnPostKilled.Add(trait.GuidStrForEvents(0), OnTerritoryCardPostKilled, PRIORITY);
        }
        async UniTask OnTerritoryCardPostKilled(object sender, ITableEntrySource source)
        {
            BattleFieldCard terrCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(terrCard.Territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;
            if (terrCard == trait.Owner) return;

            await trait.AnimActivation();
            await trait.Owner.health.AdjustValueScale(HEALTH_REL_INCREASE * trait.GetStacks(), trait);
        }
    }
}
