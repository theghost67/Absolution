using Game.Territories;
using Game.Traits;
using GreenOne;
using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс для данных карт полей. Эти карты имеют собственные характеристики и трейты (которые могут изменяться в бою).
    /// </summary>
    public abstract class FieldCard : Card
    {
        public int health;
        public int strength;
        public int moxie;
        public readonly TraitListSet traits;

        public FieldCard(string id, params string[] startTraits) : base(id, isField: true) 
        {
            health = 1;
            strength = 0;
            moxie = 0;
            traits = new TraitListSet(this);
            FillTraitListSet(startTraits);
        }
        protected FieldCard(SerializationDict dict) : base(dict)
        {
            traits = new TraitListSet(dict.DeserializeKeyAsDict("traits"));
            health = dict.DeserializeKeyAs<int>("health");
            strength = dict.DeserializeKeyAs<int>("strength");
            moxie = dict.DeserializeKeyAs<int>("moxie");
        }
        protected FieldCard(FieldCard other) : base(other)
        {
            TraitListSetCloneArgs traitsCArgs = new(this);
            traits = (TraitListSet)other.traits.Clone(traitsCArgs);
            health = other.health;
            strength = other.strength;
            moxie = other.moxie;
        }

        public static float MoxiePointsScale(in int moxie) => moxie switch
        {
            0 => 0.70f,
            1 => 0.85f,
            2 => 1.00f,
            3 => 1.15f,
            4 => 1.30f,
            5 => 1.45f,
            _ => 1.45f,
        };
        public static float PricePointsScale(in CardPrice price) => price.value switch
        {
            0 => 4.00f,
            1 => 2.00f,
            2 => 1.00f,
            3 => 0.80f,
            4 => 0.65f,
            5 => 0.50f,
            _ => 0.50f,
        };

        public override SerializationDict Serialize()
        {
            return new SerializationDict(base.Serialize())
            {
                { "traits", traits.Serialize() },
                { "health", health },
                { "strength", strength },
                { "moxie", moxie },
            };
        }
        public override TableCard CreateOnTable(Transform parent)
        {
            return new TableFieldCard(this, parent);
        }
        public override int Points()
        {
            float points = health.ClampedMin(0) + strength.ClampedMin(0) * 2;
            foreach (TraitListElement element in traits)
                points += element.Trait.Points(this, element.Stacks);
            return Convert.ToInt32(points * MoxiePointsScale(moxie) * PricePointsScale(price));
        }

        public virtual bool RangePotentialIsGuaranteed() => false;
        public virtual bool RangeSplashIsGuaranteed() => false;

        public int PointsDeltaForMoxie(int moxieAdjust)
        {
            int oldMoxie = moxie;
            int before = Points();

            moxie = oldMoxie + moxieAdjust;
            int after = Points();

            moxie = oldMoxie;
            return after - before;
        }
        public int PointsDeltaForHealth(int healthAdjust)
        {
            int oldHealth = health;
            int before = Points();

            health = oldHealth + healthAdjust;
            int after = Points();

            health = oldHealth;
            return after - before;
        }
        public int PointsDeltaForStrength(int strengthAdjust)
        {
            int oldStrength = strength;
            int before = Points();

            strength = oldStrength + strengthAdjust;
            int after = Points();

            strength = oldStrength;
            return after - before;
        }
        public int PointsDeltaForTrait(Trait trait, int stacks)
        {
            int before = Points();
            traits.AdjustStacks(trait, stacks);

            int after = Points();
            traits.AdjustStacks(trait, -stacks);

            return after - before;
        }

        void FillTraitListSet(string[] traitsStrArray)
        {
            const string FORMAT_STR = "Format: \"[1] [2] [3]\" where [1] is a trait type (p/a), [2] is a trait id and [3] is trait stacks.";
            foreach (string str in traitsStrArray)
            {
                if (str == null || str.Length == 0)
                    throw new ArgumentException($"Trait string shouldn't be null or empty.\n{FORMAT_STR}");

                string[] split = str.Split(' ');
                int splitLength = split.Length;
                if (splitLength == 0 || splitLength > 3)
                    throw new ArgumentException($"Invalid trait string split length.\n{FORMAT_STR}");

                string sType = split[0];
                string sId = split[1];
                string sStacks = split.Length == 3 ? split[2] : "1";
                int iStacks = Convert.ToInt32(sStacks);

                if (sType == "p")
                    traits.Passives.AdjustStacks(sId, iStacks);
                else if (sType == "a")
                    traits.Actives.AdjustStacks(sId, iStacks);
                else throw new ArgumentException($"Unknown trait type.\n{FORMAT_STR}");
            }
        }
    }
}
