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
    public class tUltrasonicScream : PassiveTrait
    {
        const string ID = "ultrasonic_scream";
        const int PRIORITY = 4;
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);
        static readonly TerritoryRange oppositeRange = TerritoryRange.oppositeTriple;

        public tUltrasonicScream() : base(ID)
        {
            name = "Ультразвуковой крик";
            desc = "Что ж, я только посоветую вам прикрыть уши.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.allNotSelf);
        }
        protected tUltrasonicScream(tUltrasonicScream other) : base(other) { }
        public override object Clone() => new tUltrasonicScream(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При смерти любой карты, кроме себя (П{PRIORITY})",
                    $"издаст крайне неприятный крик в сторону всех карт рядом (напротив владельца), инициатива которых будет понижена на {_moxieF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(80, stacks);
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(e.trait.GuidStr, OnTargetPostKilled, PRIORITY);
            else e.target.OnPostKilled.Remove(e.trait.GuidStr);
        }
        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(target.Territory);

            if (trait == null) return;
            if (trait.Owner.Field == null) return;
            if (target == trait.Owner) return;

            BattleFieldCard[] cards = trait.Owner.Territory.Fields(trait.Owner.Field.pos, oppositeRange).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int value = (int)-_moxieF.Value(trait);
            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.Moxie.AdjustValue(value, trait);
        }
    }
}
