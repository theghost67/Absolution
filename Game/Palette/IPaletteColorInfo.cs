using DG.Tweening;
using System;
using UnityEngine;

namespace Game.Palette
{
    /// <summary>
    /// Интерфейс, реализующий объект как объект, содержащий данные цвета из палитры.
    /// </summary>
    public interface IPaletteColorInfo // NOTE: do NOT use DOVirtual on any properties here
    {
        public string Hex { get; }
        public int Index { get; }
        public Color ColorDef { get; set; } 
        public Color ColorCur { get; set; }
        public Color ColorSet { set; }

        public Tween DOColorCur(Func<Color> from, Func<Color> to, float duration);
        public Tween DOColorDef(Func<Color> from, Func<Color> to, float duration); 
    }
}
