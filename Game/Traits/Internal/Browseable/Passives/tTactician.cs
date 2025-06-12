using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTactician : PassiveTrait
    {
        const string ID = "tactician";
        const string KEY = "turn_one";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tTactician() : base(ID)
        {
            name = "Тактик";
            desc = "Мастерски владеет тактическими приёмами.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tTactician(tTactician other) : base(other) { }
        public override object Clone() => new tTactician(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>В начале каждого второго хода на территории</color>\nУвеличивает инициативу владельца на {_moxieF.Format(args.stacks, true)}.";
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;

            if (!trait.Storage.ContainsKey(KEY))
            {
                trait.Storage.Add(KEY, null);
                return;
            }

            await trait.AnimActivation();
            await trait.Owner.Moxie.AdjustValue(_moxieF.Value(trait.GetStacks()), trait);
            trait.Storage.Remove(KEY);
        }
    }
}
