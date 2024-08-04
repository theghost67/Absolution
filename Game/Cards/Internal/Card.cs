using Game.Palette;
using GreenOne;
using System;
using System.Text;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный базовый класс для данных игровой карты.
    /// </summary>
    public abstract class Card : Unique, ISerializable, ICloneable
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
        }
        protected Card(SerializationDict dict) : base()
        {
            id = dict.DeserializeKeyAs<string>("id");
            name = dict.DeserializeKeyAs<string>("name");
            desc = dict.DeserializeKeyAs<string>("desc");
            spritePath = dict.DeserializeKeyAs<string>("spritePath");
            rarity = dict.DeserializeKeyAs<Rarity>("rarity");
            tags = dict.DeserializeKeyAs<CardTag>("tags");
            price = new CardPrice(dict.DeserializeKeyAsDict("price"));
            frequency = dict.DeserializeKeyAs<float>("frequency");
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

        public virtual SerializationDict Serialize()
        {
            return new SerializationDict()
            { 
                { "id", id },
                { "name", name },
                { "spritePath", spritePath },
                { "rarity", rarity },
                { "tags", tags },
                { "price", price.Serialize() },
                { "price", frequency },
            };
        }
        public virtual string DescRich(ITableCard card) => DescRichBase(card, "");

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

        // add string from this method to the end of DescRich(ITableCard) string
        protected static string DescRichBase(ITableCard card, string contents)
        {
            StringBuilder sb = new();
            Card data = card.Data;

            sb.Append($"<size=150%>{data.name}</size>\n<i>");
            switch (data.rarity)
            {
                case Rarity.None: sb.Append("Обычная"); break;
                case Rarity.Rare: sb.Append("Редкая"); break;
                case Rarity.Epic: sb.Append("Особая"); break;
                default: throw new NotSupportedException();
            }
            if (data.isField)
                 sb.Append(" карта поля</i>\n\n");
            else sb.Append(" карта способности</i>\n\n");

            if (contents != "")
            {
                sb.Append(contents);
                sb.Append("\n\n");
            }

            string colorHex = ColorPalette.C3.Hex;
            sb.Append($"<color={colorHex}>");

            if (card is BattleFieldCard bCard && bCard.Field != null)
            {
                int turnAge = bCard.TurnAge;
                if (turnAge > 0)
                     sb.Append($"Установлена: {turnAge} х. назад\n\n");
                else sb.Append("Установлена: на этом ходу\n\n");
            }

            if (string.IsNullOrEmpty(data.desc))
                return sb.ToString();

            sb.Append($"<i>«{data.desc}»");
            return sb.ToString();
        }
    }
}
