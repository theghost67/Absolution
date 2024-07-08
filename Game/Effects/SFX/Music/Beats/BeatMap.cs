using System.Collections;
using System.Collections.Generic;

namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий "карту" битов, т.е., с какой периодичностью будут вызываться события <see cref="SFX.OnBeat"/>.
    /// </summary>
    public class BeatMap : IReadOnlyList<Beat>
    {
        public int Count => _list.Count;
        public int MaxIntensity => _maxIntensity;
        public float LastPos => _appendPos;
        public float Bpm => _bpm;
        public float BpmScale => 60 / _bpm;
        public float Delay => _delay;

        readonly float _bpm;
        readonly float _delay;
        readonly List<Beat> _list;
        int _maxIntensity;
        float _appendPos;

        public BeatMap(float bpm, float delay) 
        {
            _bpm = bpm;
            _delay = delay;
            _appendPos = 1;
            _list = new List<Beat>(); 
        }
        public Beat this[int index] => _list[index];

        public void Add(BeatInstruction instruction)
        {
            instruction.AppendTo(this);
        }
        public void Add(Beat beat)
        {
            _list.Add(beat);
            _appendPos += beat.length;
            _maxIntensity = beat.intensity > _maxIntensity ? beat.intensity : _maxIntensity;
        }

        public IEnumerator<Beat> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
