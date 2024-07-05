using System;
using Unity.Mathematics;

namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий одну из инструкций бит-мапа. Добавляет повторяемую очередь битов в <see cref="BeatMap"/> с указанной интенсивностью.<br/>
    /// При указании <see cref="intensityRepeats"/>, периодически будет повышать интенсивность бита, сохраняя длину и очередь битов.
    /// </summary>
    public class BeatInstruction_Repeat : BeatInstruction
    {
        public readonly int count;
        public readonly int length;
        public readonly int intensity;
        public readonly int2[] intensityRepeats; // every 'x' beat, intensity will change to 'y'
                                                 // if repeat overlaps with other repeats in array, it will take max intensity

        public BeatInstruction_Repeat(int count, int length, int intensity) : this(count, length, intensity, null) { this.count = count; }
        public BeatInstruction_Repeat(int count, int length, int intensity, int2[] intensityRepeats) : base() 
        { 
            this.count = count; 
            this.length = length;
            this.intensity = intensity;
            this.intensityRepeats = intensityRepeats ?? Array.Empty<int2>();

            if (intensityRepeats == null) return;
            foreach (int2 repeat in intensityRepeats)
            {
                if (repeat.x < 2)
                    throw new ArgumentException("repeat.x min value is 2.");
            }
        }

        public override void AppendTo(BeatMap map)
        {
            for (int i = 1; i < count + 1; i++)
            {
                int maxIntensity = intensity;
                for (int j = 0; j < intensityRepeats.Length; j++)
                {
                    int2 repeat = intensityRepeats[j];
                    if (i % repeat.x == 0 && maxIntensity < repeat.y)
                        maxIntensity = repeat.y;
                }
                map.Add(length, maxIntensity);
            }
        }
    }
}
