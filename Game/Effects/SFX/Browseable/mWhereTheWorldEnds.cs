namespace Game.Effects
{
    public class mWhereTheWorldEnds : Music
    {
        const string ID = "where_the_world_ends";

        class BeatInstruction_Verse1a : BeatInstruction
        {
            public readonly int intensity;
            public BeatInstruction_Verse1a(int intensity) { this.intensity = intensity; }
            public override void AppendTo(BeatMap map)
            {
                for (int i = 0; i < 3; i++)
                    map.Add(new Beat(map, 1, intensity));

                map.Add(new Beat(map, 1, 0));
                for (int i = 0; i < 3; i++)
                    map.Add(new Beat(map, 1, intensity));

                map.Add(new Beat(map, 1, 0));
                for (int i = 0; i < 7; i++)
                    map.Add(new Beat(map, 1, intensity));
            }
        }
        class BeatInstruction_Verse1b : BeatInstruction
        {
            public readonly int intensity;
            public BeatInstruction_Verse1b(int intensity) { this.intensity = intensity; }
            public override void AppendTo(BeatMap map)
            {
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity, BeatFlags.PeakOne));

                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity, BeatFlags.PeakTwo));

                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity, BeatFlags.PeakOne));

                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity));
                map.Add(new Beat(map, 1, intensity, BeatFlags.PeakTwo));
            }
        }
        class BeatInstruction_Verse2 : BeatInstruction
        {
            public readonly int intensity;
            public BeatInstruction_Verse2(int intensity) { this.intensity = intensity; }
            public override void AppendTo(BeatMap map)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                        map.Add(new Beat(map, 1, intensity));
                    map.Add(new Beat(map, 1, intensity, i % 2 == 0 ? BeatFlags.PeakOne : BeatFlags.PeakTwo));
                }
            }
        }

        public mWhereTheWorldEnds() : base(ID) { }

        protected override BeatMap CreateBeatMap() => new(100f, 0.30f)
        {
            new BeatInstruction_Repeat(13, 1, 0),

            new BeatInstruction_Verse1a(1),
            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(3),
            new BeatInstruction_Single(1, 0),

            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1b(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1b(2),
            new BeatInstruction_Verse1b(2),

            new BeatInstruction_Verse2(1),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1a(1),
            new BeatInstruction_Single(1, 0),

            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(3),

            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(3),

            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Single(2, 1),
            new BeatInstruction_Single(2, 1),
            new BeatInstruction_Single(2, 1),
            new BeatInstruction_Single(1, 1),

            new BeatInstruction_Verse1a(1),
            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(3),
            new BeatInstruction_Single(1, 0),

            new BeatInstruction_Verse2(1),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(3),

            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse1b(2),
            new BeatInstruction_Verse1b(2),

            new BeatInstruction_Verse2(1),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1a(1),
            new BeatInstruction_Single(1, 0),

            new BeatInstruction_Verse2(1),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1b(2),
            new BeatInstruction_Verse1b(2),

            new BeatInstruction_Verse2(1),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),
            new BeatInstruction_Verse2(2),

            new BeatInstruction_Verse1a(1),
            new BeatInstruction_Single(1, 0),
            new BeatInstruction_Verse1a(1),
            //new BeatInstruction_Single(1, 0),
            //new BeatInstruction_Single(1, 2),
        };
    }
}
