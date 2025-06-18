using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMultipaw : PassiveTrait
    {
        const string ID = "multipaw";
        const string KEY = ID;
        static readonly TraitStatFormula _strength = new(true, 1.00f, 0.00f);

        public tMultipaw() : base(ID)
        {
            name = Translator.GetString("trait_multipaw_1");
            desc = Translator.GetString("trait_multipaw_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tMultipaw(tMultipaw other) : base(other) { }
        public override object Clone() => new tMultipaw(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_multipaw_3", _strength.Format(args.stacks, true));

        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                await trait.Owner.Strength.AdjustValueScale(-_strength.Value(trait.GetStacks()), trait, trait.GuidStr);
                if (trait.Owner.IsKilled) return;
                trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnInitiationPostSent);
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            }
            else if (trait.WasRemoved(e))
            {
                await trait.Owner.Strength.RevertValueScale(trait.GuidStr);
                if (trait.Owner.IsKilled) return;
                trait.Owner.OnInitiationPostSent.Remove(trait.GuidStr);
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
            }
        }
        async UniTask OnInitiationPostSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Storage.ContainsKey(KEY)) return;

            trait.Storage[KEY] = 1;
            await trait.AnimActivation();
            await owner.Territory.Initiations.EnqueueAndAwait(owner.CreateInitiation());
        }
        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.IsKilled) return;
            trait.Storage.Remove(KEY);
        }
    }
}
