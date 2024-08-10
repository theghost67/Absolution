using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tReflection : PassiveTrait
    {
        const string ID = "reflection";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _strengthF = new(true, 0.75f, 0.25f);

        public tReflection() : base(ID)
        {
            name = "Отражение";
            desc = "Посмотри на себя. Стоило ли оно того?";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tReflection(tReflection other) : base(other) { }
        public override object Clone() => new tReflection(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После атаки на владельца (П{PRIORITY})",
                    $"Атакует атакующего в ответ с силой, равной {_strengthF.Format(trait)} от атаки."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(30, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            if (owner.IsKilled) return;
            if (e.Sender.IsKilled || e.Sender.Field == null) return;

            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            int strength = (e.strength * _strengthF.Value(trait)).Rounded();
            BattleInitiationSendArgs initiation = new(owner, strength, true, false, e.Sender.Field);
            await trait.AnimActivation();
            owner.Territory.Initiations.EnqueueAndRun(initiation);
        }
    }
}
