using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBoom : PassiveTrait
    {
        const string ID = "boom";
        static readonly TraitStatFormula _strengthF = new(false, 0, 4);

        public tBoom() : base(ID)
        {
            name = Translator.GetString("trait_boom_1");
            desc = Translator.GetString("trait_boom_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerRadiusSmall);
        }
        protected tBoom(tBoom other) : base(other) { }
        public override object Clone() => new tBoom(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_boom_3", _strengthF.Format(args.stacks));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(10, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;

            float damage = _strengthF.Value(trait.GetStacks());
            await trait.AnimActivation();
            BattleFieldCard[] cards = terr.Fields(trait.Field.pos, trait.Data.range.potential).WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard card in cards)
                await card.Health.AdjustValue(-damage, trait);
            await trait.Owner.TryKill(BattleKillMode.Default, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
