namespace Game.Effects
{
    public class mQuantumLoop : Music
    {
        const string ID = "quantum_loop";
        public mQuantumLoop() : base(ID) { }
        protected override BeatMap CreateBeatMap() => new(60f, 0f) { };
    }
}
