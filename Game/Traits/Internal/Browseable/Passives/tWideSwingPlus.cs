using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWideSwingPlus : PassiveTrait
    {
        const string ID = "wide_swing_plus";
        static readonly TerritoryRange _range = TerritoryRange.ownerAll;

        public tWideSwingPlus() : base(ID)
        {
            name = Translator.GetString("trait_wide_swing_plus_1");
            desc = Translator.GetString("trait_wide_swing_plus_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tWideSwingPlus(tWideSwingPlus other) : base(other) { }
        public override object Clone() => new tWideSwingPlus(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_wide_swing_plus_3");

        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.Receivers.Count != 1) return;

            await trait.AnimActivation();
            BattleField singleField = e.Receivers[0];
            IEnumerable<BattleField> fields = owner.Territory.Fields(singleField.pos, _range);

            e.ClearReceivers();
            foreach (BattleField field in fields)
                e.AddReceiver(field);

            await e.Strength.SetValue((float)e.Strength / e.Receivers.Count, trait);
        }
    }
}
