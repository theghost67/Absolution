using Game.Palette;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный базовый класс для данных игровой карты.
    /// </summary>
    public abstract class Card : Unique, IDescriptive, ICloneable
    {
        public const float POINTS_MAX = 9999;
        public readonly string id;
        public readonly bool isField; // used to remove 'is' type checks as there are only two derived types 

        public string name;
        public string desc;
        public string spritePath;
        public Rarity rarity;
        public CardTag tags;
        public CardPrice price;
        public float frequency; // 0-1

        public Card(string id, bool isField) : base()
        {
            this.id = id;
            this.isField = isField;
            this.spritePath = $"Sprites/Cards/Portraits/{id}";
            this.frequency = 1f;
        }
        protected Card(Card other) : base(other.Guid)
        {
            id = other.id;
            isField = other.isField;

            name = string.Copy(other.name);
            desc = string.Copy(other.desc);
            spritePath = other.spritePath;
            rarity = other.rarity;
            tags = other.tags;
            price = other.price;
            frequency = other.frequency;
        }

        public abstract object Clone();
        public object CloneAsNew()
        {
            Card clone = (Card)Clone();
            clone.GiveNewGuid();
            return clone;
        }

        public abstract TableCard CreateOnTable(Transform parent);
        public abstract float Points(); // 1 point = 1 hp
        public float PointsDeltaForPrice(int priceAdjust, CardCurrency currency = null)
        {
            int oldPrice = price.value;
            CardCurrency oldCurrency = currency;
            float before = Points();

            price.value = oldPrice + priceAdjust;
            if (currency != null) price.currency = currency;
            float after = Points();

            price.value = oldPrice;
            if (currency != null) price.currency = oldCurrency;
            return after - before;
        }

        public string DescDynamic(CardDescriptiveArgs args)
        {
            string contentsFormat = DescContentsFormat(args);
            string internalFormat = DescInternalFormat(args, contentsFormat);
            return internalFormat;
        }
        public virtual DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            // used to show multiple tooltips on mouse over
            // link type (to card/to trait) will be determined by value type

            // key = trait_id link
            // value = args passed to trait_id.DescDynamic() function

            return DescLinkCollection.empty;
        }
        public bool HasTableEquivalent()
        {
            return true;
        }

        protected virtual string DescContentsFormat(CardDescriptiveArgs args)
        {
            // main block for description, can use args.custom created from DescParamsCreator here
            return "";
        }
        private string DescInternalFormat(CardDescriptiveArgs args, string contents)
        {
            if (args.linkStats.Length != 4 || !args.isCard)
                throw new ArgumentException(nameof(args));

            StringBuilder sb = new();
            sb.Append($"<size=150%>{name}</size>\n");
			if (isField)
			{
				switch (rarity)
				{
					case Rarity.None: sb.Append(Translator.GetString("card_1")); break;
					case Rarity.Rare: sb.Append(Translator.GetString("card_2")); break;
					case Rarity.Epic: sb.Append(Translator.GetString("card_3")); break;
					default: throw new NotSupportedException();
				}
			}
			else
			{
				switch (rarity)
				{
					case Rarity.None: sb.Append(Translator.GetString("card_4")); break;
					case Rarity.Rare: sb.Append(Translator.GetString("card_5")); break;
					case Rarity.Epic: sb.Append(Translator.GetString("card_6")); break;
					default: throw new NotSupportedException();
				}
			}

			sb.Append("\n\n");
            sb.Append(contents);
            if (contents != "")
                sb.Append("\n\n");

            if (args.linkFormat)
            {
                if (args.linkStats[0] >= 0) sb.AppendLine(Translator.GetString("card_7", price.currency.name, args.linkStats[0]));
                if (args.linkStats[1] >= 0) sb.AppendLine(Translator.GetString("card_8", args.linkStats[1]));
                if (args.linkStats[2] >= 0) sb.AppendLine(Translator.GetString("card_9", args.linkStats[2]));
                if (args.linkStats[3] >= 0) sb.AppendLine(Translator.GetString("card_10", args.linkStats[3]));

                if (args.linkTraits.Length == 0)
                {
                    sb.Append(Translator.GetString("card_11"));
                    return sb.ToString();
                }
                sb.Append(Translator.GetString("card_12"));
                foreach (TraitStacksPair pair in args.linkTraits)
                {
                    Trait trait = TraitBrowser.GetTrait(pair.id);
                    Color color = (trait.isPassive ? ColorPalette.CP : ColorPalette.CA).ColorCur;
                    sb.Append(trait.name.Colored(color));
                    sb.Append($" x{pair.stacks}, ");
                }
                sb.Remove(sb.Length - 2, 2);
                return sb.ToString();
            }

            sb.Append($"<color={ColorPalette.C2.Hex}>");
            bool extraLine = false;

            int turnAge = args.table?.TurnAge ?? -1;
            if (turnAge > 0)
            {
                sb.Append(Translator.GetString("card_13", turnAge));
                extraLine = true;
            }
            else if (turnAge == 0)
            {
                sb.Append(Translator.GetString("card_14"));
                extraLine = true;
            }

            if (string.IsNullOrEmpty(desc))
                return sb.ToString();
            if (extraLine)
                sb.Append('\n');

            sb.Append($"<i>«{desc}»");
            return sb.ToString();
        }

        string IDescriptive.DescDynamic(DescriptiveArgs args)
        {
            return DescDynamic((CardDescriptiveArgs)args);
        }
        DescLinkCollection IDescriptive.DescLinks(DescriptiveArgs args)
        {
            return DescLinks((CardDescriptiveArgs)args);
        }
    }
}
