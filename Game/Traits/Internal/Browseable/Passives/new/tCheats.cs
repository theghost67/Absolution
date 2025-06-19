using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCheats : PassiveTrait
    {
        const string ID = "cheats";

        public tCheats() : base(ID)
        {
            name = Translator.GetString("trait_cheats_1");
            desc = Translator.GetString("trait_cheats_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tCheats(tCheats other) : base(other) { }
        public override object Clone() => new tCheats(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_cheats_3");
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
            if (trait == null || trait.Owner == null) return;
            if (owner.Drawer == null) return;

            bool hadField = owner.LastField != null;
            bool hasField = owner.Field != null && !owner.IsKilled;
            bool sideChanged = hadField && hasField && owner.LastField.Side != owner.Field.Side;
            if (!sideChanged && (hadField == hasField)) return;

            if (hasField)
            {
                await trait.AnimActivation();
                trait.Side.Opposite.Drawer.SleeveIsVisible = true;
                if (sideChanged)
                    trait.Side.Drawer.SleeveIsVisible = false;
            }
            else
            {
                await trait.AnimDeactivation();
                trait.Side.Opposite.Drawer.SleeveIsVisible = false;
            }
        }
    }
}
