namespace Game.Effects
{
    public class m9thDimension : Music
    {
        const string ID = "9th_dimension";
        public m9thDimension() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
