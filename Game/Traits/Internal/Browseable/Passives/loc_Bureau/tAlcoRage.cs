using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoRage : PassiveTrait
    {
        const string ID = "alco_rage";
        const int PRIORITY = 5;
        const float HEALTH_ABS_RESTORE = 0.50f;
        const int MOXIE_ABS_DECREASE = 2;

        public tAlcoRage() : base(ID)
        {
            name = "Алкогольное безумие";
            desc = "Вы разнесли мне половину бара!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tAlcoRage(tAlcoRage other) : base(other) { }
        public override object Clone() => new tAlcoRage(this);

        public override string DescRich(ITableTrait trait)
        {
            float healthEffect = HEALTH_ABS_RESTORE * 100 * trait.GetStacks();
            float moxieEffect = MOXIE_ABS_DECREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После убийства вражеской карты владельцем (П{PRIORITY})",
                    $"Восстанавливает себе <u>{healthEffect}%</u> здоровья и уменьшает свою инициативу на <u>{moxieEffect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 24 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnKillConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (e.victim.Side == owner.Side) return;

            int stacks = trait.GetStacks();
            float health = owner.Data.health * HEALTH_ABS_RESTORE * stacks;
            float moxie = -MOXIE_ABS_DECREASE * stacks;

            await trait.AnimActivation();
            await owner.health.AdjustValue(health, trait);
            await owner.moxie.AdjustValue(moxie, trait);
        }
    }
}
