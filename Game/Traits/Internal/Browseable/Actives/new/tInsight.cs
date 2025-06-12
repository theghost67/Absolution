using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tInsight : ActiveTrait
    {
        const string ID = "insight";
        const string TRAIT_ID = "immortality";
        static readonly TraitStatFormula _turnStacks = new(false, 1, 0);
        static readonly TraitStatFormula _useStacks = new(false, 2, 0);
        static readonly TraitStatFormula _requiredStacks = new(false, 3, 0);

        public tInsight() : base(ID)
        {
            name = "Озарение";
            desc = "Сейчас тебя озарит и ты молча уйдёшь, верно?";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tInsight(tInsight other) : base(other) { }
        public override object Clone() => new tInsight(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>Постоянный эффект в бою на территории</color>\nДаёт {_turnStacks.Format(args.stacks)} зарядов каждый ход. " +
                   $"Активация навыка возможна только при накоплении {_requiredStacks.Format(args.stacks)} зарядов.\n\n" +
                   $"<color>При активации на союзной карте рядом</color>\nДаёт цели <nobr><u>{traitName}</u></nobr>, тратит {_useStacks.Format(args.stacks)} зарядов.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.traitStacks >= _requiredStacks.ValueInt(e.traitStacks);
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            await target.Card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
            await trait.AdjustStacks(-_useStacks.ValueInt(e.traitStacks), trait);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;
            await trait.AdjustStacks(1, trait);
        }
    }
}
