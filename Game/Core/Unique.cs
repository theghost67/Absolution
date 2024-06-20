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

        public Unique()
        {
            _guid = NewGuid;
            _guidStr = _guid.ToString();
        }
        public Unique(int guid) 
        { 
            _guid = guid;
            _guidStr = guid.ToString();
        }
        public Unique(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                _guid = NewGuid;
                _guidStr = _guid.ToString();
            }
            else
            {
                _guid = guid.GetHashCode();
                _guidStr = guid;
            }
        }

        public bool Equals(Unique other)
        {
            return _guid.Equals(other._guid);
        }
        public int CompareTo(Unique other)
        {
            return _guid.CompareTo(other._guid);
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
    }
}
