using Game.Cards;
using Game.Territories;
using System;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий случайное событие локации, изменяющее события локации и частоту появления различных сущностей.
    /// </summary>
    public class LocationEvent
    {
        public const int THREAT_LEVEL_MAX = 5;

        public readonly string id;
        public readonly int threatLevel;

        public string name;
        public string desc;

        public LocationFieldCardMod[] fieldCardsMods;
        public LocationFloatCardMod[] floatCardsMods;
        public LocationPassiveTraitMod[] passiveTraitsMods;
        public LocationActiveTraitMod[] activeTraitsMods;
        public LocationPlaceMod[] placesMods;

        public TravelerEvents travelerEvents;
        public PlacesEvents placesEvents;
        public TerritoryEvents territoryEvents;
        public FieldCardEvents fieldCardEvents;
        public FloatCardEvents floatCardsEvents;

        /// <summary>
        /// Класс, содержащий события, вызываемые в классе <see cref="Traveler"/>.
        /// </summary>
        public sealed class TravelerEvents
        {
            public Action OnTravelStart;
            public Action OnTravelEnd;

            public Action<TravelerDeckPreEventArgs> OnDeckPreCreated;
            public Action<TravelerDeckPostEventArgs> OnDeckPostCreated;

            public Action<TravelerCardPreEventArgs> OnCardPreCreated;
            public Action<TravelerCardPostEventArgs> OnCardPostCreated;
        }
        /// <summary>
        /// Класс, содержащий события, вызываемые в классе <see cref="PlaceMenu"/>.
        /// </summary>
        public sealed class PlacesEvents
        {
            public Action OnPlaceEnter;
            public Action OnPlaceLeave;
        }
        /// <summary>
        /// Класс, содержащий события, вызываемые в классах <see cref="TableTerritory"/> и <see cref="BattleTerritory"/>.
        /// </summary>
        public sealed class TerritoryEvents
        {
            public Action<TableField> OnAnyCardAttachedTo;
            public Action<TableField> OnAnyCardDetatchedFrom;

            public Action OnStartPhase;
            public Action OnEnemyPhase;
            public Action OnPlayerPhase;
            public Action OnEndPhase;

            public Action OnNextPhase;
            public Action OnProcessingStart;
            public Action OnProcessingEnd;
        }
        /// <summary>
        /// Класс, содержащий события, вызываемые в классах <see cref="TableFieldCard"/> и <see cref="BattleFieldCard"/>.
        /// </summary>
        public sealed class FieldCardEvents
        {
            public Action OnHealthPreSet;
            public Action OnHealthPostSet;
            public Action OnStrengthPreSet;
            public Action OnStrengthPostSet;
            public Action OnMoxiePreSet;
            public Action OnMoxiePostSet;

            public Action OnAppear;
            public Action OnTryToAttachToField;
            public Action OnPreAttachedToField;
            public Action OnPostAttachedToField;
            public Action OnPreKilled;
            public Action OnPostKilled;
            public Action OnKill;
            public Action OnInitiationPreSent;
            public Action OnInitiationPostSent;
            public Action OnInitiationConfirmed;
            public Action OnInitiationPreReceived;
            public Action OnInitiationPostReceived;
        }
        /// <summary>
        /// Класс, содержащий события, вызываемые в классах <see cref="TableFloatCard"/> и <see cref="BattleFloatCard"/>.
        /// </summary>
        public sealed class FloatCardEvents
        {
            public Action OnUsed;
        }

        public LocationEvent(string id, int threatLevel)
        {
            if (threatLevel < 1 || threatLevel > 5)
                throw new ArgumentOutOfRangeException("Threat level should be in range [1, 5].");

            this.id = id;
            this.threatLevel = threatLevel;
        }

        // TODO[IMPORTANT]: add 'TrySub' methods that will check if event group is null and subscribe events to entity
    }
}
