using Cysharp.Threading.Tasks;
using Game.Territories;
using System;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMinistryRat : PassiveTrait
    {
        const string ID = "ministry_rat";
        const int PRIORITY = 5;

        public tMinistryRat() : base(ID)
        {
            name = "Министерская крыса";
            desc = "Вы все одинаковые.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tMinistryRat(tMinistryRat other) : base(other) { }
        public override object Clone() => new tMinistryRat(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале хода, когда остаётся один на своей стороне территории (П{PRIORITY})",
                    $"переходит на вражеское поле напротив, если оно не занято. Тратит все заряды."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryEndPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryEndPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;
            if (trait.Owner.Field.Opposite.Card != null) return;
            if (trait.Side.Fields().WithCard().ToArray().Length != 1) return;

            await trait.AnimActivation();
            await trait.Owner.TryAttachToField(trait.Owner.Field.Opposite, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
