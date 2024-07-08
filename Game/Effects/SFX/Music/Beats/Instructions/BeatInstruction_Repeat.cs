using System.Collections.Generic;

namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий одну из инструкций бит-мапа. Добавляет повторяемую очередь битов в <see cref="BeatMap"/> с указанной интенсивностью.<br/>
    /// При указании <see cref="custom"/>, периодически будет повышать интенсивность бита, сохраняя длину и очередь битов.
    /// </summary>
    public class BeatInstruction_Repeat : BeatInstruction
    {
        public readonly int count;
        public readonly int length;
        public readonly int intensity;
        public readonly BeatFlags flags;
        public readonly CustomRepeats custom; // every 'x' beat, intensity/flags will change

        public class CustomRepeats : HashSet<CustomRepeat> { }
        public class CustomRepeat
        {
            public readonly int xFrequency;
            public readonly int intensity;
            public readonly BeatFlags flags;
            public CustomRepeat(int xFrequency, int intensity, BeatFlags flags)
            {
                this.xFrequency = xFrequency;
                this.intensity = intensity;
                this.flags = flags;
            }
        }

        public BeatInstruction_Repeat(int count, int length, int intensity, BeatFlags flags = default) : base() 
        { 
            this.count = count; 
            this.length = length;
            this.intensity = intensity;
            this.flags = flags;
            this.custom = new CustomRepeats();
        }

        public override void AppendTo(BeatMap map)
        {
            for (int i = 1; i < count + 1; i++)
            {
                int overlapIntensity = intensity;
                BeatFlags overlapFlags = flags;
                foreach (CustomRepeat repeat in custom)
                {
                    if (i % repeat.xFrequency == 0)
                    {
                        if (overlapIntensity < repeat.intensity)
                            overlapIntensity = repeat.intensity;
                        overlapFlags |= repeat.flags;
                    }
                }
                map.Add(new Beat(map, length, overlapIntensity, flags));
            }
        }
        public BeatInstruction_Repeat WithCustomRepeats(CustomRepeat[] repeats)
        {
            foreach (CustomRepeat repeat in repeats)
                custom.Add(repeat);
            return this;
        }
    }
}
