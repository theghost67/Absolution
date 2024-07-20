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
        const float DAMAGE_REL_INCREASE = 1.00f;

        public tTriptocainum() : base(ID)
        {
            name = "Триптокаинум";
            desc = "";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tTriptocainum(tTriptocainum other) : base(other) { }
        public override object Clone() => new tTriptocainum(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = DAMAGE_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании",
                    $"Увеличивает силу владельца на <u>{effect}%</u>. Владелец умрёт (игнор. здоровья) сразу после совершения атакующей инициации (П{PRIORITY}). Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 120 * Mathf.Pow(stacks - 1, 2);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Opposite.Card == null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            BattleActiveTrait trait = (BattleActiveTrait)e.trait;

            float strength = DAMAGE_REL_INCREASE * 100 * trait.GetStacks();
            await trait.Owner.strength.AdjustValueScale(strength, trait);
            trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnInitiationPostSent);
            await trait.SetStacks(0, trait.Side);
        }

        static async UniTask OnInitiationPostSent(object sender, BattleInitiationSendArgs sArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await owner.Kill(BattleKillMode.IgnoreHealthRestore, trait);
        }
    }
}
