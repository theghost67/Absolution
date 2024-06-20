using Game.Menus;
using MyBox;
using System;
using System.Collections.Generic;

namespace Game.Environment
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех данных локаций, их миссий и мест в игре.
    /// </summary>
    [Obsolete("Rewrite same as CardBrowser/TraitBrowser")] public static class EnvironmentBrowser
    {
        // DO NOT SET VALUES: stored data used when creating new locations, events and places
        public static IReadOnlyDictionary<string, Location> Locations => _locations;
        public static IReadOnlyDictionary<string, LocationEvent> LocationEvents => _locationEvents;
        public static IReadOnlyDictionary<string, LocationPlace> LocationPlaces => _locationPlaces;

        static readonly Dictionary<string, Location> _locations = new();
        static readonly Dictionary<string, LocationEvent> _locationEvents = new();
        static readonly Dictionary<string, LocationPlace> _locationPlaces = new();
        static readonly Dictionary<string, LocationEvent>[] _locationEventsByThreatLvl = new Dictionary<string, LocationEvent>[LocationEvent.THREAT_LEVEL_MAX];

        public static void Initialize()
        {
            for (int i = 0; i < LocationEvent.THREAT_LEVEL_MAX; i++)
                _locationEventsByThreatLvl[i] = new Dictionary<string, LocationEvent>();

            AddLoc(new Location("college", 1)
            {
                name = "Гнилой колледж",
                stage = 8,
                fieldCards = new string[]
                {
                    "granny",
                    "hysteric",
                    "barbarian",
                    "bread",
                    "sausages",

                    "cyber_cutlet",
                    "pigeon",
                    "student",
                    "russian",
                    "vavulov",

                    "gavenko",
                    "moshev",
                    "michael",
                    "principals_office",
                    "canteen",
                },
                floatCards = new string[]
                {
                    "kotovs_syndrome",
                    "kalenskiy_protocol",
                    "vavulization",
                    "delete_due_to_uselessness",
                },
                places = new string[]
                {
                    "battle",
                    //"trader",
                    //"library",
                    //"writer",
                }
            });

            AddLocPlace(new LocationPlace("battle")
            {
                name = "Бой",
                desc = "Блуждающий монстр, убийца, страж - здесь будет бой.",
                frequency = 1f,
                menuCreator = () => new BattlePlaceMenu(),
            });
        }

        public static LocationEvent GetLocationEvent(int threatLevel)
        {
            Dictionary<string, LocationEvent> events = _locationEventsByThreatLvl[threatLevel - 1];
            if (events.Count != 0)
                 return events.Values.GetRandom();
            else return null;
        }

        static void AddLoc(Location location)
        {
            _locations.Add(location.id, location);
        }
        static void AddLocEvent(LocationEvent @event)
        {
            _locationEvents.Add(@event.id, @event);
            _locationEventsByThreatLvl[@event.threatLevel - 1].Add(@event.id, @event);
        }
        static void AddLocPlace(LocationPlace place)
        {
            _locationPlaces.Add(place.id, place);
        }

        static LocationEvent InitEvent_Dishonesty_1()
        {
            LocationEvent le = new("dishonesty", threatLevel: 1);
            le.travelerEvents = new LocationEvent.TravelerEvents();
            le.travelerEvents.OnDeckPreCreated += e =>
            {
                if (e.deckType != Traveler.EntityType.Enemy)
                    return;

                Traveler.fieldsFrequencies.Add("some_field_card_id", 0);
            };
            le.travelerEvents.OnDeckPostCreated += e =>
            {
                if (e.deckType != Traveler.EntityType.Enemy)
                    return;

                Traveler.fieldsFrequencies.Remove("some_field_card_id");
            };
            return le;
        }
    }
}
