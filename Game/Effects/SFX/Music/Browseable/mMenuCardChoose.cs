namespace Game.Effects
{
    internal class mMenuCardChoose : Music
    {
        const string ID = "card_choose";
        const string PATH = "SFX/Music/" + ID;

        public mMenuCardChoose() : base(ID, PATH) { }
        protected override BeatMap CreateBeatMap() => new(21.25f, -0.42f)
        {
            new BeatInstruction_Repeat(48, 1, 2),
            new BeatInstruction_Repeat(16, 1, 0),
            new BeatInstruction_Repeat(48, 1, 2),
        };
    }
}
