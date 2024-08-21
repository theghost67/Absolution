using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoRage : PassiveTrait
    {
        const string ID = "alco_rage";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _healthF = new(true, 0.00f, 0.33f);
        static readonly TraitStatFormula _moxieF = new(false, 0, 2);

        public tAlcoRage() : base(ID)
        {
            name = "Алкогольное безумие";
            desc = "Вы разнесли мне половину бара!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tAlcoRage(tAlcoRage other) : base(other) { }
        public override object Clone() => new tAlcoRage(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После убийства вражеской карты владельцем (П{PRIORITY})</color>\n" +
                   $"Восстанавливает себе {_healthF.Format(args.stacks)} здоровья и уменьшает свою инициативу на {_moxieF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(18, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnKillConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (e.victim.Side == owner.Side) return;

            int stacks = trait.GetStacks();
            float health = owner.Data.health * _healthF.Value(stacks);
            float moxie = -_moxieF.Value(stacks);

            await trait.AnimActivation();
            await owner.Health.AdjustValue(health, trait);
            await owner.Moxie.AdjustValue(moxie, trait);
        }
    }
}
