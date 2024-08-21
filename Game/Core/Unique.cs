using System;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий уникальный объект.
    /// </summary>
    public abstract class Unique : IUnique, IEquatable<Unique>, IComparable<Unique>
    {
        public static int NewGuid => _nextGuid++;
        public static string NewGuidStr => _nextGuid++.ToString();
        static int _nextGuid;

        public int Guid => _guid;
        public string GuidStr => _guidStr;

        int _guid;
        string _guidStr;

        public Unique() : this(NewGuid) { }
        public Unique(int guid) 
        { 
            _guid = guid;
            _guidStr = guid.ToString();
        }

        public static bool operator ==(Unique left, Unique right)
        {
            if (left is null)
                 return right is null;
            else return left.Equals(right);
        }
        public static bool operator !=(Unique left, Unique right)
        {
            return !(left == right);
        }

        public bool Equals(Unique other)
        {
            if (other is null)
                return false;
            else return _guid.Equals(other._guid);
        }
        public int CompareTo(Unique other)
        {
            if (other is null)
                return 1;
            else return _guid.CompareTo(other._guid);
        }

        public override bool Equals(object obj)
        {
            if (obj is Unique unique)
                 return Equals(unique);
            else return false;
        }
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        protected void GiveNewGuid()
        {
            _guid = NewGuid;
            _guidStr = _guid.ToString();
        }
    }
}
