using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDoctor : PassiveTrait
    {
        const string ID = "doctor";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);

        public tDoctor() : base(ID)
        {
            name = Translator.GetString("trait_doctor_1");
            desc = Translator.GetString("trait_doctor_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tDoctor(tDoctor other) : base(other) { }
        public override object Clone() => new tDoctor(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_doctor_3", _healthF.Format(args.stacks));

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
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null || trait.Owner.IsKilled) return;

            BattleFieldCard[] cards = trait.Territory.Fields(trait.Owner.Field.pos, trait.Data.range.potential).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            await trait.AnimActivation();

            BattleFieldCard card;
            if (cards.Length == 1)
                card = cards[0];
            else if (cards[0].Health <= cards[1].Health)
                card = cards[0];
            else card = cards[1];

            int health = _healthF.ValueInt(trait.GetStacks());
            await card.Health.AdjustValue(health, trait);
        }
    }
}
