using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tImmortality : PassiveTrait
    {
        const string ID = "immortality";
        static readonly TraitStatFormula _stacksRemove = new(false, 1, 0);

        public tImmortality() : base(ID)
        {
            name = Translator.GetString("trait_immortality_1");
            desc = Translator.GetString("trait_immortality_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tImmortality(tImmortality other) : base(other) { }
        public override object Clone() => new tImmortality(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_immortality_3", _stacksRemove.Format(args.stacks, true));

        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
                trait.Owner.CanBeKilled = false;
            }
            else if (trait.WasRemoved(e))
            {
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
                trait.Owner.CanBeKilled = true;
            }
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            await trait.AdjustStacks(-_stacksRemove.ValueInt(trait.GetStacks()), trait);
        }
    }
}
