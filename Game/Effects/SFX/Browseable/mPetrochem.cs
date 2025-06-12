namespace Game.Effects
{
    public class mPetrochem : Music
    {
        const string ID = "petrochem";
        public mPetrochem() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
