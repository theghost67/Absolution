namespace Game.Effects
{
    public class mTerminal : Music
    {
        const string ID = "terminal";
        public mTerminal() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
