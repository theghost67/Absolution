using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScorchingFlame : PassiveTrait
    {
        const string ID = "scorching_flame";
        static readonly TraitStatFormula _strengthF = new(true, 0.25f, 0.25f);
        ITableTrait _preReceiveTrait;

        public tScorchingFlame() : base(ID)
        {
            name = "Обжигающее пламя";
            desc = "О, какой ты горячий...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tScorchingFlame(tScorchingFlame other) : base(other) { }
        public override object Clone() => new tScorchingFlame(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После получения атаки владельцем</color>\nМгновенно наносит атакующему урон, равный {_strengthF.Format(args.stacks, true)}. " +
                   $"Активируется даже после смерти владельца.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.5f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived);
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
            }
        }
        async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            ITableTrait trait = owner.Traits.Any(ID);
            _preReceiveTrait = trait;
        }
        async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            if (_preReceiveTrait == null || e.Sender.IsKilled) return;

            ITableTrait trait = _preReceiveTrait;
            int damage = (int)Mathf.Ceil(e.Strength * _strengthF.Value(trait.GetStacks()));
            await trait.AnimActivationShort();
            await e.Sender.Health.AdjustValue(-damage, trait);
        }
    }
}
