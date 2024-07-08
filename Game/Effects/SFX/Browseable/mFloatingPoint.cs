namespace Game.Effects
{
    public class mFloatingPoint : Music
    {
        const string ID = "floating_point";
        public mFloatingPoint() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
