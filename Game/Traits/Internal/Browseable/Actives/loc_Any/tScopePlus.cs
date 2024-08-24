﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using Unity.Mathematics;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScopePlus : ActiveTrait
    {
        const string ID = "scope_plus";
        const int PRIORITY = 2;

        public tScopePlus() : base(ID)
        {
            name = "Прицел+";
            desc = "Глубокий вдох и... Ещё один глубокий вдох и...";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tScopePlus(tScopePlus other) : base(other) { }
        public override object Clone() => new tScopePlus(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При использовании на любом вражеском поле</color>\nУказанная цель станет целью будущих атак владельца (П{PRIORITY}).\n\n" +
                   $"<color>При перемещении владельца (П{PRIORITY})</color>\nДеактивирует эффект данного навыка.";
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            trait.Storage[trait.GuidStr] = e.target.pos;
            owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent, PRIORITY);
            owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached, PRIORITY);
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
            if (trait == null) return;

            await trait.AnimDeactivation();
            OnRemove(trait);
        }
        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

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
