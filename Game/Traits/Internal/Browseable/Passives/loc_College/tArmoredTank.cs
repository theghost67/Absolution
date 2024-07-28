using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tArmoredTank : PassiveTrait
    {
        const string ID = "armored_tank";
        const int PRIORITY = 5;
        const float DAMAGE_REL_DECREASE = 0.50f;

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
                new($"Перед атакой на владельца (П{PRIORITY})",
                    $"уменьшает силу атаки на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 40 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (e.strength < 0) return;

            await trait.AnimActivation();
            await e.strength.AdjustValueScale(-DAMAGE_REL_DECREASE * trait.GetStacks(), trait);
        }
    }
}
