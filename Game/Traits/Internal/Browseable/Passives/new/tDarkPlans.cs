using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDarkPlans : PassiveTrait
    {
        const string ID = "dark_plans";

        public tDarkPlans() : base(ID)
        {
            name = "Тёмные планы";
            desc = "Она готова предать даже своих друзей для достижения её цели.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tDarkPlans(tDarkPlans other) : base(other) { }
        public override object Clone() => new tDarkPlans(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После установки на поле</color>\nКрадёт 1 ед. инициативы со всех карт на союзной территории. Тратит все заряды.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Territory.Fields(owner.Field.pos, TerritoryRange.ownerAllNotSelf).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int moxie = 0;
            foreach (BattleFieldCard card in cards)
            {
                string entryId = trait.GuidStr;
                await card.Moxie.AdjustValue(-1, trait, entryId);
                moxie += (int)-card.Moxie.EntryValue(entryId);
            }

            if (owner.IsKilled) return;

            await trait.AnimActivation();
            await owner.Moxie.AdjustValue(moxie, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
