﻿using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tGrannyAlliance : PassiveTrait
    {
        const string ID = "granny_alliance";
        const int PRIORITY = 8;
        const float STRENGTH_REL_INCREASE = 0.50f;

        public tGrannyAlliance() : base(ID)
        {
            name = "Альянс бабуль";
            desc = "Чем нас больше, тем мы страшнее!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tGrannyAlliance(tGrannyAlliance other) : base(other) { }
        public override object Clone() => new tGrannyAlliance(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = STRENGTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>Бабуся</i> рядом с владельцем (П{PRIORITY})",
                    $"увеличивает силу владельца на <u>{effect}%</u>, однако, при смерти одной из такой карт, владелец так же умрёт от инициатора."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 12 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            BattlePassiveTrait trait = (BattlePassiveTrait)e.trait;
            string entryId = $"{trait.Guid}/{e.target.Guid}";

            if (e.target.Data.id != "granny") return;
            if (e.canSeeTarget)
            {
                e.target.OnPostKilled.Add(OnTargetCardKilled, PRIORITY);
                await trait.AnimActivation();
                await trait.Owner.strength.AdjustValueScale(STRENGTH_REL_INCREASE * trait.GetStacks(), trait, entryId);
            }
            else
            {
                e.target.OnPostKilled.Remove(OnTargetCardKilled);
                await trait.AnimDeactivation();
                await trait.Owner.strength.RevertValueScale(entryId);
            }
        }

        async UniTask OnTargetCardKilled(object sender, ITableEntrySource source)
        {
            BattleFieldCard observingCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(observingCard.Territory);
            if (trait == null) return;
            await trait.Owner.Kill(BattleKillMode.Default, source);
        }
    }
}
