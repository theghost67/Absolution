using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Palette;
using Game.Territories;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Абстрактный базовый класс для данных игровых навыков.
    /// </summary>
    public abstract class Trait : Unique, IDescriptive, ICloneable
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
        private TableFinder _traitFinder;                  // in event handlers subscribed in OnStacksChanged (or other) fucntions

        public Trait(string id, bool isPassive) : base()
        {
            this.id = id;
            this.isPassive = isPassive;
            this.spritePath = $"Sprites/Traits/Icons/{id}";
            this.frequency = 1f;

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
            foreach (KeyValuePair<string, object> pair in other.storage)
                storage.Add(pair.Key, pair.Value);

            _traitFinder = other._traitFinder;
        }

        public bool Equals(Trait other)
        {
            return id == other.id;
        }

        public abstract TableTrait CreateOnTable(Transform parent);
        public abstract object Clone();
        public object CloneAsNew()
        {
            Trait clone = (Trait)Clone();
            clone.GiveNewGuid();
            return clone;
        }

        public virtual BattleWeight Weight(IBattleTrait trait)   // used to increase weight in battle above default value, 1 absolute weight = 1 hp
        {
            return default;
        }
        public virtual float Points(FieldCard owner, int stacks) // used to increase trait points required to add another stack of this trait to a card
        {
            return 0;
        }

        public virtual async UniTask OnStacksChanged(TableTraitStacksSetArgs e) // invokes either on table or in battle (check 'isInBattle' field)
        {
            _traitFinder = e.trait.Finder;
            if (!e.isInBattle) return;
            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasRemoved(e))
                await trait.Area.SetObserveTargets(false);
        }
        public virtual UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e) // invokes only in battle (when target was seen/unseen)
        {
            return UniTask.CompletedTask;
        }

        public string DescDynamic(TraitDescriptiveArgs args)
        {
            if (args.stacks <= 0)
                args.stacks = 1;
            string contentsFormat = DescContentsFormat(args);
            string internalFormat = DescInternalFormat(args, contentsFormat);
            return internalFormat;
        }
        public virtual DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            // used to show multiple tooltips on mouse over
            // link type (to card/to trait) will be determined by value type

            // key = id link
            // value = args passed to AnyBrowser.Get(id).DescDynamic() function

            return DescLinkCollection.empty;
        }
        public bool HasTableEquivalent()
        {
            return true;
        }

        protected virtual string DescContentsFormat(TraitDescriptiveArgs args)
        {
            // main block for description, can use args.custom created from DescParamsCreator here
            return "";
        }
        private string DescInternalFormat(TraitDescriptiveArgs args, string contents)
        {
            if (args.stacks <= 0 || args.isCard)
                throw new ArgumentException(nameof(args));

            StringBuilder sb = new();
            sb.Append($"<size=150%>{name} x{args.stacks}</size>\n<i>");

            string headColorHex = isPassive ? ColorPalette.CP.Hex : ColorPalette.CA.Hex;
            contents = contents.Replace("<color>", $"<color={headColorHex}>");

            switch (rarity)
            {
                case Rarity.None: sb.Append("Обычный"); break;
                case Rarity.Rare: sb.Append("Редкий"); break;
                case Rarity.Epic: sb.Append("Особый"); break;
                default: throw new NotSupportedException();
            }

            string staticStr = tags.HasFlag(TraitTag.Static) ? " (статичный)" : "";
            if (isPassive)
                 sb.Append($" пассивный навык{staticStr}</i>\n\n");
            else sb.Append($" активный навык{staticStr}</i>\n\n");

            sb.Append(contents);
            if (args.linkFormat) return sb.ToString();
            if (contents != "") 
                sb.Append("\n\n");

            sb.Append($"<color={ColorPalette.C2.Hex}>");
            bool extraLine = false;

            int turnsPassed = args.table?.TurnAge ?? -1;
            if (turnsPassed > 0)
            {
                sb.Append($"Навык наложен: {turnsPassed} х. назад\n");
                extraLine = true;
            }
            int turnsDelay = args.turnsDelay;
            if (turnsDelay > 0)
            {
                sb.Append($"Навык перезаряжается: {turnsDelay} х.\n");
                extraLine = true;
            }

            if (string.IsNullOrEmpty(desc))
                return sb.ToString();
            if (extraLine)
                sb.Append('\n');

            sb.Append($"<i>«{desc}»");
            return sb.ToString();
        }

        protected static float TurnDelayToWeightMod(float delay)
        {
            if (delay > 0)
                 return 2f / Mathf.Pow(2, delay);
            else return 2f;
        }
        protected static float PointsLinear(float perStack, int stacks, int freeStacks = 1)
        {
            if (stacks > freeStacks)
                return perStack * (stacks - freeStacks);
            else return 0;
        }
        protected static float PointsExponential(float perStack, int stacks, int freeStacks = 1, float p = 2)
        {
            if (stacks > freeStacks)
                return perStack * Mathf.Pow(stacks - freeStacks, p);
            else return 0;
        }

        string IDescriptive.DescDynamic(DescriptiveArgs args)
        {
            return DescDynamic((TraitDescriptiveArgs)args);
        }
        DescLinkCollection IDescriptive.DescLinks(DescriptiveArgs args)
        {
            return DescLinks((TraitDescriptiveArgs)args);
        }
    }
}
