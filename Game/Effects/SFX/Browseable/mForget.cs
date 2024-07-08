namespace Game.Effects
{
    public class mForget : Music
    {
        const string ID = "forget";
        public mForget() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
