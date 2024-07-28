using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTriptocainum : ActiveTrait
    {
        const string ID = "triptocainum";
        const int PRIORITY = 7;
        const int COOLDOWN = 3;
        const float DAMAGE_REL_INCREASE = 0.50f;

        public tTriptocainum() : base(ID)
        {
            name = "Триптокаинум";
            desc = "КОКАИНУМ!";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tTriptocainum(tTriptocainum other) : base(other) { }
        public override object Clone() => new tTriptocainum(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = DAMAGE_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании",
                    $"Увеличивает силу владельца на <u>{effect}%</u>. Владелец умрёт (игнор. здоровья) сразу после совершения атаки (П{PRIORITY}). Перезарядка: 3 х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 80 * Mathf.Pow(stacks - 1, 2);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(0, 0.1f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            IBattleTrait trait = (IBattleTrait)e.trait;

            float strength = DAMAGE_REL_INCREASE * trait.GetStacks();
            await trait.Owner.strength.AdjustValueScale(strength, trait);
            trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnInitiationPostSent);
            trait.Storage.turnsDelay += COOLDOWN;
        }

        static async UniTask OnInitiationPostSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await owner.TryKill(BattleKillMode.IgnoreHealthRestore, trait);
        }
    }
}
