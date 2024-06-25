using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tArmoredTank : PassiveTrait
    {
        const string ID = "armored_tank";
        const int PRIORITY = 8;
        const float DAMAGE_REL_DECREASE = 1.00f;

        public tArmoredTank() : base(ID)
        {
            name = "Бронетанк";
            desc = "Да у него непробиваемый слой защиты! Что это может быть?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tArmoredTank(tArmoredTank other) : base(other) { }
        public override object Clone() => new tArmoredTank(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = DAMAGE_REL_DECREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед получением атакующей инициации владельцем (П{PRIORITY})",
                    $"уменьшает силу инициации на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 20 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(OnOwnerInitiationPreReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(OnOwnerInitiationPreReceived);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs rArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await rArgs.strength.AdjustValueRel(-DAMAGE_REL_DECREASE * trait.GetStacks(), trait);
        }
    }
}
