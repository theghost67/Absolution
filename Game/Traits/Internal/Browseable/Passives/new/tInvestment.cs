using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tInvestment : PassiveTrait
    {
        const string ID = "investment";
        const string KEY_ACTIVATIONS = "activations";
        const string KEY_TURN = "turn";
        static readonly TraitStatFormula _limitF = new(false, 3, 0);

        public tInvestment() : base(ID)
        {
            name = "Инвестировал...";
            desc = "Инвестировал в говно.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tInvestment(tInvestment other) : base(other) { }
        public override object Clone() => new tInvestment(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После первой установки владельца на поле</color>\nПонижает стоимость владельца до нуля.\n\n" +
                   $"<color>В начале каждого второго хода на территории</color>\nПовышает стоимость владельца на 1 ед. (не более {_limitF.Format(args.stacks)}).\n\n" +
                   $"<color>После смерти владельца</color>\nДаёт стороне-владельцу стоимость владельца на момент смерти.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                if (trait.Owner.IsKilled) return;
                trait.Storage[KEY_ACTIVATIONS] = 0;
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnPostKilled);
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            }
            else if (trait.WasRemoved(e))
            {
                if (trait.Owner.IsKilled) return;
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
            }
        }

        async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || !owner.FirstFieldAttachment) return;

            await trait.AnimActivation();
            await owner.Price.SetValue(0, trait);
        }
        async UniTask OnPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            if (owner.Data.price.currency.id == "gold")
                 await trait.Side.Gold.AdjustValue(owner.Price, trait);
            else await trait.Side.Ether.AdjustValue(owner.Price, trait);
        }
        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            if (!trait.Storage.ContainsKey(KEY_TURN))
            {
                trait.Storage.Add(KEY_TURN, null);
                return;
            }

            int activations = (int)trait.Storage[KEY_ACTIVATIONS];
            if (activations >= _limitF.ValueInt(trait.GetStacks()))
                return;

            await trait.AnimActivation();
            trait.Storage[KEY_ACTIVATIONS] = +1;
            trait.Storage.Remove(KEY_TURN);
            await trait.Owner.Price.AdjustValue(1, trait);
        }
    }
}
