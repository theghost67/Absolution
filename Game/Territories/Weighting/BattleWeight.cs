using System;
using System.Collections.Generic;

namespace Game.Territories
{
    /// <summary>
    /// Структура, представляющая вес сущности во время сражения (см. <see cref="IBattleWeighty"/>).<br/> 
    /// Этот вес прибавляется к стороне, к которой принадлежит эта сущность.<br/>
    /// Тип может использоваться как дополнительный вес к уже существующему типу <see cref="BattleWeight"/>.
    /// </summary>
    public readonly struct BattleWeight : IEquatable<BattleWeight>
    {
        public static readonly BattleWeight one = new(1, 0);
        public static readonly BattleWeight zero = new(0, 0);
        public static readonly BattleWeight negative = new(-1, 0);

        public readonly float absolute;
        public readonly float relative;

        public BattleWeight(float absolute) : this(absolute, 0) { }
        public BattleWeight(float absolute, float relative)
        {
            this.absolute = absolute;
            this.relative = relative;
        }

        public static float Float(float startAbsValue, params BattleWeight[] weights)
        {
            return Float(startAbsValue, (IEnumerable<BattleWeight>)weights);
        }
        public static float Float(float startAbsValue, IEnumerable<BattleWeight> weights)
        {
            float relDelta = 0;
            foreach (BattleWeight weight in weights)
            {
                startAbsValue += weight.absolute;
                relDelta += weight.relative;
            }

            float relMod;
            if (relDelta > 0)
                 relMod = 1 * (1 + relDelta);
            else relMod = 1 / (1 - relDelta);

            return startAbsValue * relMod;
        }

        public bool Equals(BattleWeight other)
        {
            return absolute == other.absolute && relative == other.relative;
        }
    }
}
