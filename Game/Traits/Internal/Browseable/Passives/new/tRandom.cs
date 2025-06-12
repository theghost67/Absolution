using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;

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
            new(1, 4,  $"уничтожает все остальные карты (сначала вражеские)"), 
            new(2, 6,  $"уничтожает все вражеские карты"),
            new(3, 10, $"увеличивает силу всех карт на {EV3_STRENGTH_BONUS * 100}% до следующего хода"), 
            new(4, 20, $"уничтожает все союзные карты рядом"),
            new(5, 20, $"увеличивает силу союзных карт рядом на {EV5_STRENGTH_BONUS * 100}%"), 
            new(6, 20, $"увеличивает здоровье всех остальных союзных карт на {EV6_HEALTH_RESTORE * 100}%"), 
            new(7, 20, $"уменьшает инициативу всех остальных карт на {EV7_MOXIE_DEBUFF} ед."),
        };

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
            name = "ТОЛЬКО НЕ ЭТО";
            desc = "ВЫ ЗАЧЕМ ЕГО РАЗБЛОКИРОВАЛИ???";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.all);
        }
        protected tRandom(tRandom other) : base(other) { }
        public override object Clone() => new tRandom(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>В начале хода на территории, активирует <b>случайное</b> событие из нижеперечисленных</color>\n" +
                   $"1. [{_events[0].probability}%] {_events[0].desc}\n" +
                   $"2. [{_events[1].probability}%] {_events[1].desc}\n" +
                   $"3. [{_events[2].probability}%] {_events[2].desc}\n" +
                   $"4. [{_events[3].probability}%] {_events[3].desc}\n" +
                   $"5. [{_events[4].probability}%] {_events[4].desc}\n" +
                   $"6. [{_events[5].probability}%] {_events[5].desc}\n" +
                   $"7. [{_events[6].probability}%] {_events[6].desc}";
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

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }
        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.IsKilled || owner.Field == null) return;

            await trait.AnimActivation();
            int lastEventId = (int)trait.Storage[LAST_EV_KEY];
            if (lastEventId == 3)
                terr.ContinuousAttachHandler_Remove(trait.GuidStr, Ev3_ContinuousAttach_Remove);

            IEnumerable<BattleFieldCard> cards;
            Event newEvent = _events.GetWeightedRandom(e => e.probability);
            trait.Storage[LAST_EV_KEY] = newEvent.id;

            await trait.AnimActivationShort($"СОБЫТИЕ #{newEvent.id}");
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
                    terr.ContinuousAttachHandler_Add(trait.GuidStr, Ev3_ContinuousAttach_Add);
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

        UniTask Ev3_ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (!card.IsKilled)
                 return card.Strength.AdjustValueScale(EV3_STRENGTH_BONUS, trait, trait.GuidGen(3));
            else return UniTask.CompletedTask;
        }
        UniTask Ev3_ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (!card.IsKilled)
                return card.Strength.RevertValueScale(trait.GuidGen(3));
            else return UniTask.CompletedTask;
        }
    }
}
