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

        public virtual float Value(int stacks)
        {
            if (valuePerStack == 0)
                 return valueBase;
            else return valueBase + valuePerStack * stacks;
        }
        public int ValueInt(int stacks)
        {
            if (!isRelative)
                return Value(stacks).Ceiling();
            else throw new System.InvalidCastException("Relative stats cannot be casted to int.");
        }
        public string Format(int stacks, bool noDot = false)
        {
            if (isRelative)
            {
                if (DoFormatWithColor())
                     return $"<nobr><color>{(Value(stacks) * 100).Rounded()}%</color></nobr>";
                else return $"<nobr>{(Value(stacks) * 100).Rounded()}%</nobr>";
            }
            else
            {
                string postfix = noDot ? Translator.GetString("trait_stat_formula_1") : Translator.GetString("trait_stat_formula_2");
                if (DoFormatWithColor())
                     return $"<nobr><color>{Value(stacks).Rounded()} {postfix}</color></nobr>";
                else return $"<nobr>{Value(stacks).Rounded()} {postfix}</nobr>";
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
                     return Translator.GetString("trait_stat_formula_3", valuePerStack);
                else if (valuePerStack == 0)
                     return Translator.GetString("trait_stat_formula_4", valueBase);
                else return Translator.GetString("trait_stat_formula_5", valueBase, valuePerStack);
            }
        }
        protected virtual bool DoFormatWithColor()
        {
            return valuePerStack != 0;
        }
    }
}
