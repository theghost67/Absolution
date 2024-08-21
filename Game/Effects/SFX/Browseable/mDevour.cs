namespace Game.Effects
{
    public class mDevour : Music
    {
        const string ID = "devour";
        public mDevour() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
