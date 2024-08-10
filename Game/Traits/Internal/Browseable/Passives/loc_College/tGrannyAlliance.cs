﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tGrannyAlliance : PassiveTrait
    {
        const string ID = "granny_alliance";
        const string CARD_ID = "granny";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.50f);

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
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>{cardName}</i> рядом с владельцем (П{PRIORITY})",
                    $"увеличивает силу владельца на {_strengthF.Format(trait)}, однако, при смерти одной из такой карт, владелец так же умрёт от инициатора."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(20, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            string entryId = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != ID) return;
            if (e.canSeeTarget)
            {
                e.target.OnPostKilled.Add(trait.GuidStr, OnTargetPostKilled, PRIORITY);
                await trait.AnimDetectionOnSeen(e.target);
                await trait.Owner.Strength.AdjustValueScale(_strengthF.Value(trait), trait, entryId);
            }
            else
            {
                e.target.OnPostKilled.Remove(trait.GuidStr);
                await trait.AnimDetectionOnUnseen(e.target);
                await trait.Owner.Strength.RevertValueScale(entryId);
            }
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard observingCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(observingCard.Territory);
            if (trait == null) return;
            await trait.Owner.TryKill(BattleKillMode.Default, e.source);
        }
    }
}
