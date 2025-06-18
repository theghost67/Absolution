using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tRandom : PassiveTrait
    {
        const string ID = "random";
        const string LAST_EV_KEY = ID;
        const float EV3_STRENGTH_BONUS = 1.00f;
        const float EV5_STRENGTH_BONUS = 0.25f;
        const float EV6_HEALTH_RESTORE = 0.33f;
        const int EV7_MOXIE_DEBUFF = 2;

        static readonly List<Event> _events = new()
        {
            // do NOT use 0 as ID
            new(1, 4,  Translator.GetString("trait_random_1")), 
            new(2, 6,  Translator.GetString("trait_random_2")),
            new(3, 10, Translator.GetString("trait_random_3", EV3_STRENGTH_BONUS * 100)), 
            new(4, 20, Translator.GetString("trait_random_4")),
            new(5, 20, Translator.GetString("trait_random_5", EV5_STRENGTH_BONUS * 100)), 
            new(6, 20, Translator.GetString("trait_random_6", EV6_HEALTH_RESTORE * 100)), 
            new(7, 20, Translator.GetString("trait_random_7", EV7_MOXIE_DEBUFF)),
        };
        string _guid;

        private class Event
        {
            public readonly int id;
            public readonly int probability;
            public readonly string desc;

            public Event(int id, int probability, string name)
            {
                this.id = id;
                this.probability = probability;
                this.desc = name;
            }
        }

        public tRandom() : base(ID)
        {
            name = Translator.GetString("trait_random_8");
            desc = Translator.GetString("trait_random_9");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.all);
        }
        protected tRandom(tRandom other) : base(other) { _guid = other._guid; }
        public override object Clone() => new tRandom(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_random_10", _events[0].probability, _events[0].desc, _events[1].probability, _events[1].desc, _events[2].probability, _events[2].desc, _events[3].probability, _events[3].desc, _events[4].probability, _events[4].desc, _events[5].probability, _events[5].desc, _events[6].probability, _events[6].desc);

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
            trait.Storage[LAST_EV_KEY] = 0;
            _guid = trait.GuidStr;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(_guid, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(_guid);
        }
        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.IsKilled || owner.Field == null) return;

            await trait.AnimActivation();
            IEnumerable<BattleFieldCard> cards;
            Event newEvent = _events.GetWeightedRandom(e => e.probability);
            trait.Storage[LAST_EV_KEY] = newEvent.id;

            await trait.AnimActivationShort(Translator.GetString("trait_random_11", newEvent.id));
            switch (newEvent.id)
            {
                case 1:
                    cards = terr.Fields().WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards) // enemy cards are killed first
                        await card.TryKill(BattleKillMode.Default, trait);
                    break;

                case 2:
                    cards = trait.Side.Opposite.Fields().WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards)
                        await card.TryKill(BattleKillMode.Default, trait);
                    break;

                case 3:
                    await terr.ContinuousAttachHandler_Add(_guid, Ev3_ContinuousAttach_Add);
                    terr.OnStartPhase.Add(_guid, Ev3_OnTerrStartPhase);
                    break;

                case 4:
                    cards = terr.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards)
                        await card.TryKill(BattleKillMode.Default, trait);
                    break;

                case 5:
                    cards = terr.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards)
                        await card.Strength.AdjustValueScale(EV5_STRENGTH_BONUS, trait);
                    break;

                case 6:
                    cards = terr.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards)
                        await card.Health.AdjustValueScale(EV6_HEALTH_RESTORE, trait);
                    break;

                case 7:
                    cards = terr.Fields().WithCard().Select(f => f.Card);
                    foreach (BattleFieldCard card in cards)
                        await card.Moxie.AdjustValue(-EV7_MOXIE_DEBUFF, trait);
                    break;
            }
        }

        async UniTask Ev3_OnTerrStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            await terr.ContinuousAttachHandler_Remove(_guid, Ev3_ContinuousAttach_Remove);
            terr.OnStartPhase.Remove(_guid);
        }
        async UniTask Ev3_ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            await card.Strength.AdjustValueScale(EV3_STRENGTH_BONUS, trait, _guid);
        }
        async UniTask Ev3_ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            await card.Strength.RevertValueScale(_guid);
        }
    }
}
