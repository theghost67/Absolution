using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tUltrasonicScream : PassiveTrait
    {
        const string ID = "ultrasonic_scream";
        const int PRIORITY = 4;
        const int MOXIE_DECREASE = 1;

        public tUltrasonicScream() : base(ID)
        {
            name = "Ультразвуковой крик";
            desc = "Что ж, я только посоветую вам прикрыть уши.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.allNotSelf);
        }
        protected tUltrasonicScream(tUltrasonicScream other) : base(other) { }
        public override object Clone() => new tUltrasonicScream(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = MOXIE_DECREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При смерти любой карты, кроме себя (П{PRIORITY}/Т)",
                    $"издаст крайне неприятный крик в сторону карты напротив владельца, инициатива которой будет понижена на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 8 * Mathf.Pow(stacks - 1, 2);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(e.trait.GuidStrForEvents(0), OnTargetCardKilled, PRIORITY);
            else e.target.OnPostKilled.Remove(e.trait.GuidStrForEvents(0));
        }

        async UniTask OnTargetCardKilled(object sender, ITableEntrySource source)
        {
            BattleFieldCard observingCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(observingCard.Territory);

            if (trait.Owner.Field == null) return;
            BattleFieldCard ownerOppositeFieldCard = observingCard.Territory.FieldOpposite(trait.Owner.Field.pos).Card;

            if (trait == null || ownerOppositeFieldCard == null) return;
            if (observingCard == trait.Owner) return;

            ownerOppositeFieldCard.Drawer?.AnimShowSelection();
            await trait.AnimActivation();
            await trait.Owner.Drawer.transform.DOAShake(0.04f).AsyncWaitForCompletion();
            ownerOppositeFieldCard.Drawer?.AnimHideSelection();
            await ownerOppositeFieldCard.moxie.AdjustValue(-MOXIE_DECREASE * trait.GetStacks(), trait);
        }
    }
}
