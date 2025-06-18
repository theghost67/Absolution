using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTableManipulations : PassiveTrait
    {
        const string ID = "table_manipulations";
        static readonly TraitStatFormula _valueF = new(false, 1, 0);

        public tTableManipulations() : base(ID)
        {
            name = Translator.GetString("trait_table_manipulations_1");
            desc = Translator.GetString("trait_table_manipulations_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tTableManipulations(tTableManipulations other) : base(other) { }
        public override object Clone() => new tTableManipulations(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_table_manipulations_3", _valueF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(20, stacks);
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            return new(trait, 0, 0.1f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                await trait.Territory.ContinuousAttachHandler_Add(trait.GuidStr, ContinuousAttach_Add, trait.Owner);
            else if (trait.WasRemoved(e))
                await trait.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
        }

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null && card != trait.Owner)
                card.OnFieldPostAttached.Add(trait.GuidStr, OnCardFieldPostAttached);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null && card != trait.Owner)
                card.OnFieldPostAttached.Remove(trait.GuidStr);
        }

        async UniTask OnCardFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(card.Territory);
            if (trait == null || trait.Side != card.Side || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || !card.FirstFieldAttachment) return;

            await trait.AnimActivation();
            int value = _valueF.ValueInt(trait.GetStacks());
            if (card.Data.price.currency.id == "gold")
                 await trait.Side.Gold.AdjustValue(value, trait);
            else await trait.Side.Ether.AdjustValue(value, trait);
            await trait.AdjustStacks(-1, trait);
        }
    }
}
