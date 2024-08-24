using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPlayWithVictim : PassiveTrait
    {
        const string ID = "play_with_victim";
        const int PRIORITY = 3;
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.3333f);

        public tPlayWithVictim() : base(ID)
        {
            name = "Поиграть с жертвой";
            desc = "Мы с тобой остались совсем одни...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tPlayWithVictim(tPlayWithVictim other) : base(other) { }
        public override object Clone() => new tPlayWithVictim(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После совершения атаки на карту владельцем (П{PRIORITY})</color>\n" +
                   $"Уменьшает силу цели на {_strengthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.8f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (e.Receiver == null || e.Receiver.IsKilled) return;

            await trait.AnimActivation();
            await e.Receiver.Strength.AdjustValueScale(-_strengthF.Value(trait.GetStacks()), trait);
        }
    }
}
