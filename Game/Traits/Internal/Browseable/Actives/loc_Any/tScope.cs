using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using Unity.Mathematics;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScope : ActiveTrait
    {
        const string ID = "scope";
        const int PRIORITY = 2;

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
            return $"<color>При использовании на вражеской карте рядом</color>\nПеред каждой последующей атакой владельца (П{PRIORITY}), <i>цель</i> станет целью атаки.\n\n" +
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
