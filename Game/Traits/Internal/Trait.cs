using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Palette;
using Game.Territories;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный базовый класс для данных игровых навыков.
    /// </summary>
    public abstract class Trait : Unique, ISerializable, ICloneable
    {
        public readonly string id;
        public readonly bool isPassive; // used to remove 'is' type checks as there are only two derived types (PassiveTrait and ActiveTrait)

        public string name;
        public string desc;
        public string spritePath;
        public Rarity rarity;
        public TraitTag tags;
        public BattleRange range;
        public float frequency; // 0-1
        public readonly TraitStorage storage;

        protected TableFinder TraitFinder => _traitFinder; // use to find attached ITableTrait on TableTerritory/BattleTerritory
        TableFinder _traitFinder;                          // in event handlers subscribed in OnStacksChanged() method

        public Trait(string id, bool isPassive) : base()
        {
            this.id = id;
            this.isPassive = isPassive;
            this.spritePath = $"Sprites/Traits/Icons/{id}";

            range = BattleRange.none;
            storage = new TraitStorage(this);
        }
        protected Trait(Trait other) : base(other.Guid)
        {
            id = other.id;
            isPassive = other.isPassive;

            name = string.Copy(other.name);
            desc = string.Copy(other.desc);
            spritePath = string.Copy(other.spritePath);
            rarity = other.rarity;
            tags = other.tags;
            range = other.range;
            frequency = other.frequency;
            storage = new TraitStorage(this);
            foreach (KeyValuePair<int, object> pair in other.storage)
                storage.Add(pair.Key, pair.Value);

            _traitFinder = other._traitFinder;
        }

        public virtual string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, Array.Empty<TraitDescChunk>());
        }
        public virtual BattleWeight Weight(IBattleTrait trait)   // used to increase weight in battle above default value, 1 absolute weight = 1 hp
        {
            return tags.HasFlag(TraitTag.Static) ? default : new(trait.GetStacks());
        }
        public virtual float Points(FieldCard owner, int stacks) // used to increase trait points required to add another stack of this trait to a card
        {
            return 0;
        }

        public virtual async UniTask OnStacksChanged(TableTraitStacksSetArgs e) // invokes either on table or in battle (check 'isInBattle' field)
        {
            _traitFinder = e.Trait.Finder;
            if (!e.isInBattle) return;
            IBattleTrait trait = (IBattleTrait)e.Trait;
            if (trait.WasRemoved(e))
                await trait.Area.SetObserveTargets(false);
        }
        public virtual UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e) // invokes only in battle (when target was seen/unseen)
        {
            return UniTask.CompletedTask;
        }

        public bool Equals(Trait other)
        {
            return id == other.id;
        }
        public SerializationDict Serialize()
        {
            // TODO[IMPORTANT]: finish trait serialization
            throw new NotImplementedException();
        }

        public abstract TableTrait CreateOnTable(Transform parent);
        public abstract object Clone();
        public object CloneAsNew()
        {
            Trait clone = (Trait)Clone();
            clone.GiveNewGuid();
            return clone;
        }

        protected static string DescRichBase(ITableTrait trait, TraitDescChunk[] descChunks)
        {
            StringBuilder sb = new();
            Trait data = trait.Data;
            string headColorHex = data.isPassive ? ColorPalette.Passive.Hex : ColorPalette.Active.Hex;

            sb.Append($"<size=150%>{data.name} x{trait.GetStacks()}</size>\n<i>");
            switch (data.rarity)
            {
                case Rarity.None: sb.Append("Обычный"); break;
                case Rarity.Rare: sb.Append("Редкий"); break;
                case Rarity.Epic: sb.Append("Особый"); break;
                default: throw new NotSupportedException();
            }
            if (data.isPassive)
                 sb.Append(" пассивный навык</i>\n\n");
            else sb.Append(" активный навык</i>\n\n");

            for (int i = 0; i < descChunks.Length; i++)
            {
                TraitDescChunk descChunk = descChunks[i];
                if (i != 0)
                     sb.Append($"\n\n<color={headColorHex}>{descChunk.header}</color>\n{descChunk.contents}");
                else sb.Append($"<color={headColorHex}>{descChunk.header}</color>\n{descChunk.contents}");
            }

            string colorHex = ColorPalette.GetColorInfo(2).Hex;
            if (descChunks.Length != 0)
                 sb.Append($"\n\n<color={colorHex}>");
            else sb.Append($"<color={colorHex}>");
            bool hasTurnText = false;

            int turnsPassed = trait.Storage.turnsPassed;
            if (turnsPassed > 0)
            {
                sb.Append($"Наложен: {turnsPassed} х. назад\n");
                hasTurnText = true;
            }

            int turnsDelay = trait.Storage.turnsDelay;
            if (turnsDelay > 0)
            {
                sb.Append($"Перезарядка: {turnsDelay} х.\n");
                hasTurnText = true;
            }

            if (hasTurnText)
                 sb.Append($"\n<i>«{data.desc}»");
            else sb.Append($"<i>«{data.desc}»");
            return sb.ToString();
        }
        protected static float TurnDelayToWeightMod(float delay)
        {
            if (delay > 0)
                 return 2f / Mathf.Pow(2, delay);
            else return 2f;
        }
    }
}
