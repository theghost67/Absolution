using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;
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
        const int MOXIE_DECREASE_PER_STACK = 1;
        static readonly TerritoryRange oppositeRange = TerritoryRange.oppositeTriple;

        public tUltrasonicScream() : base(ID)
        {
            name = "Ультразвуковой крик";
            desc = "Что ж, я только посоветую вам прикрыть уши.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.allNotSelf);
        }
        protected tUltrasonicScream(tUltrasonicScream other) : base(other) { }
        public override object Clone() => new tUltrasonicScream(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = MOXIE_DECREASE_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При смерти любой карты, кроме себя (П{PRIORITY}/Т)",
                    $"издаст крайне неприятный крик в сторону всех карт рядом (напротив владельца), инициатива которых будет понижена на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 16 * Mathf.Pow(stacks - 1, 2f);
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

            if (trait == null) return;
            if (trait.Owner.Field == null) return;
            if (observingCard == trait.Owner) return;

            BattleFieldCard[] cards = trait.Owner.Territory.Fields(trait.Owner.Field.pos, oppositeRange).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int moxie = -MOXIE_DECREASE_PER_STACK * trait.GetStacks();
            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.moxie.AdjustValue(moxie, trait);
        }
    }
}
