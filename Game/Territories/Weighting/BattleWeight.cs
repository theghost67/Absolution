using System;
using System.Collections.Generic;

namespace Game.Territories
{
    /// <summary>
    /// Структура, представляющая вес сущности во время сражения (см. <see cref="IBattleWeighty"/>).<br/> 
    /// Этот вес прибавляется к стороне, к которой принадлежит эта сущность.<br/>
    /// Тип может использоваться как дополнительный вес к уже существующему типу <see cref="BattleWeight"/>.
    /// </summary>
    public struct BattleWeight : IEquatable<BattleWeight>
    {
        public readonly int guid;
        public readonly float Total => Float(0, this);

        public float absolute;
        public float relative;

        public BattleWeight(IBattleObject obj) : this(obj, 0, 0) { }
        public BattleWeight(IBattleObject obj, float absolute) : this(obj, absolute, 0) { }
        public BattleWeight(IBattleObject obj, float absolute, float relative)
        {
            guid = obj.Guid;
            this.absolute = absolute;
            this.relative = relative;
        }

        public static BattleWeight One(IBattleObject obj) => new(obj, 1, 0);
        public static BattleWeight Zero(IBattleObject obj) => new(obj, 0, 0);
        public static BattleWeight Negative(IBattleObject obj) => new(obj, -1, 0);

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

        public readonly bool Equals(BattleWeight other)
        {
            return absolute == other.absolute && relative == other.relative;
        }
        public override string ToString()
        {
            return $"[{guid}] abs: {absolute}, rel: {relative}";
        }
    }
}
