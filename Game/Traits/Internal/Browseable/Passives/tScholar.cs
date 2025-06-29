using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScholar : PassiveTrait
    {
        const string ID = "scholar";
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tScholar() : base(ID)
        {
            name = Translator.GetString("trait_scholar_1");
            desc = Translator.GetString("trait_scholar_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tScholar(tScholar other) : base(other) { }
        public override object Clone() => new tScholar(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_scholar_3", _strengthF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
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
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Add(trait.GuidStr, OnPostKilled);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            BattleTerritory terr = card.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || card.Side == trait.Side) return;

            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (killer == null) return;
            if (killer == trait.Owner) return;
            if (killer.Side != trait.Side) return;

            await trait.AnimActivation();
            await trait.Owner.Strength.AdjustValue(_strengthF.ValueInt(trait.GetStacks()), trait);
        }
    }
}
