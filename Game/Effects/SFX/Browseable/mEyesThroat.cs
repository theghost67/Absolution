namespace Game.Effects
{
    public class mEyesThroat : Music
    {
        const string ID = "eyes_throat";
        public mEyesThroat() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
