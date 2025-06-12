using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScope : ActiveTrait
    {
        const string ID = "scope";

        public tScope() : base(ID)
        {
            name = "Прицел";
            desc = "Глубокий вдох и...";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tScope(tScope other) : base(other) { }
        public override object Clone() => new tScope(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на вражеском поле рядом</color>\nУказанная цель станет целью будущих атак владельца.\n\n" +
                   $"<color>При перемещении владельца</color>\nДеактивирует эффект данного навыка, если цель более не досягаема.";
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            trait.Storage[trait.GuidStr] = e.target.pos;
            owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasRemoved(e))
                OnRemove(trait);
        }

        static async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            bool hasKey = trait.Storage.TryGetValue(trait.GuidStr, out object posObj);
            if (!hasKey) return;

            int2 pos = (int2)posObj;
            IEnumerable<int2> fieldsPositions = owner.Territory.Fields(owner.Field.pos, trait.Data.range.potential).Select(f => f.pos);
            if (fieldsPositions.Contains(pos)) return;

            await trait.AnimDeactivation();
            OnRemove(trait);
        }
        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            int2 pos = (int2)trait.Storage[trait.GuidStr];
            BattleField field = owner.Territory.Field(pos);

            e.ClearReceivers();
            e.AddReceiver(field);
        }
        static void OnRemove(IBattleTrait trait)
        {
            trait.Storage.Remove(trait.GuidStr);
            trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }
    }
}
