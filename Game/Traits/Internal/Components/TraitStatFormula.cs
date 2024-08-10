using GreenOne;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий какую-либо характеристику, используемую внутри методов класса <see cref="Trait"/>,<br/>
    /// значение которой зависит от состояния навыка на столе (см. <see cref="ITableTrait"/>).
    /// </summary>
    public class TraitStatFormula
    {
        public readonly bool isRelative;
        public readonly float valueBase;
        public readonly float valuePerStack;

        public TraitStatFormula(bool isRelative, float valueBase, float valuePerStack) 
        {
            this.isRelative = isRelative;
            this.valueBase = valueBase;
            this.valuePerStack = valuePerStack;
        }

        public int ValueInt(ITableTrait trait)
        {
            if (!isRelative)
                return (int)Value(trait);
            else throw new System.InvalidCastException("Relative stats cannot be casted to int.");
        }
        public int ValueInt(ITableTrait trait, int stacks)
        {
            if (!isRelative)
                return (int)Value(trait, stacks);
            else throw new System.InvalidCastException("Relative stats cannot be casted to int.");
        }

        public float Value(ITableTrait trait)
        {
            return Value(trait, trait.GetStacks());
        }
        public virtual float Value(ITableTrait trait, int stacks)
        {
            if (valuePerStack == 0)
                 return valueBase;
            else return valueBase + valuePerStack * stacks;
        }

        public string Format(ITableTrait trait)
        {
            return Format(trait, trait.GetStacks());
        }
        public string Format(ITableTrait trait, int stacks)
        {
            if (isRelative)
            {
                if (DoFormatWithOutline())
                     return $"<u>{(Value(trait, stacks) * 100).Rounded()}%</u>";
                else return $"{(Value(trait, stacks) * 100).Rounded()}%";
            }
            else
            {
                if (DoFormatWithOutline())
                     return $"<u>{Value(trait, stacks).Rounded()} ед</u>";
                else return $"{Value(trait, stacks).Rounded()} ед";
            }
        }

        public override string ToString()
        {
            if (isRelative)
            {
                if (valueBase == 0)
                    return $"X * {valuePerStack * 100}%";
                else if (valuePerStack == 0)
                    return $"{valueBase * 100}%";
                else return $"{valueBase * 100}% + X * {valuePerStack * 100}%";
            }
            else
            {
                if (valueBase == 0)
                     return $"X * {valuePerStack} ед";
                else if (valuePerStack == 0)
                     return $"{valueBase} ед";
                else return $"{valueBase} ед + X * {valuePerStack} ед";
            }
        }
        protected virtual bool DoFormatWithOutline()
        {
            return valuePerStack != 0;
        }
    }
}
