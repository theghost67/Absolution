using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tUnpleasantScent : PassiveTrait
    {
        const string ID = "unpleasant_scent";
        const int PRIORITY = 4;
        const int MOXIE_DECREASE = 1;

        public tUnpleasantScent() : base(ID)
        {
            name = "Неприятный аромат";
            desc = "Ах, какой неповторимый запах помойки.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.oppositeAll);
        }
        protected tUnpleasantScent(tUnpleasantScent other) : base(other) { }
        public override object Clone() => new tUnpleasantScent(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = MOXIE_DECREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти владельца (П{PRIORITY})",
                    $"уменьшает инициативу инициатора на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 4 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPostKilled(object sender, ITableEntrySource source)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            BattleFieldCard killer = source.AsBattleFieldCard();
            if (killer == null) return;

            await trait.AnimActivation();
            await killer.moxie.AdjustValue(-MOXIE_DECREASE * trait.GetStacks(), trait);
        }
    }
}
