using Cysharp.Threading.Tasks;
using System.Linq;
using Game.Cards;
using Game.Territories;
using System;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAmazement : PassiveTrait
    {
        const string ID = "amazement";
        const string COUNTER_ID = "counter";
        const string TRAIT_ID = "stun";

        public tAmazement() : base(ID)
        {
            name = "Потрясение";
            desc = "Вау. Я потрясён.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tAmazement(tAmazement other) : base(other) { }
        public override object Clone() => new tAmazement(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При убийстве трёх вражеских карт любым источником за один ход</color>\nДаёт <nobr><u>{traitName}</u></nobr> всем остальным картам на вражеской территории.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Territory.ContinuousAttachHandler_Add(trait.GuidStr, ContinuousAttach_Add);
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
            }
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;

            trait.Storage[COUNTER_ID] = 0;
        }
        async UniTask OnCardPostKilled(object sender, EventArgs e)
        {
            BattleFieldCard victim = (BattleFieldCard)sender;
            BattleTerritory terr = victim.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;
            if (victim.Side == trait.Side) return;

            if (trait.Storage.ContainsKey(COUNTER_ID))
                 trait.Storage[COUNTER_ID] = ((int)trait.Storage[COUNTER_ID]) + 1;
            else trait.Storage[COUNTER_ID] = 1;

            if ((int)trait.Storage[COUNTER_ID] != 3) return;
            await trait.AnimActivation();

            IEnumerable<BattleFieldCard> cards = trait.Side.Opposite.Fields().WithCard().Select(f => f.Card);
            foreach (BattleFieldCard card in cards)
            {
                if (!card.IsKilled)
                    await card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
            }
        }

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null)
                card.OnPostKilled.Add(trait.GuidStr, OnCardPostKilled);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null)
                card.OnPostKilled.Remove(trait.GuidStr);
        }
    }
}
