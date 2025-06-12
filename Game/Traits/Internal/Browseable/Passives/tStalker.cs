using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStalker : PassiveTrait
    {
        const string ID = "stalker";
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.25f);

        public tStalker() : base(ID)
        {
            name = "Преследователь";
            desc = "...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tStalker(tStalker other) : base(other) { }
        public override object Clone() => new tStalker(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После совершения атаки владельцем</color>\n" +
                   $"Если владелец не атаковал цель ранее, увеличивает свою силу на {_strengthF.Format(args.stacks, true)}. Эффект делится на количество целей при атаке.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(20, stacks, 1, 1.8f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.ReceiverCard == null) return;
            if (trait.Storage.ContainsKey(e.ReceiverField.GuidStr)) return;

            int stacks = trait.GetStacks();
            float effect = _strengthF.Value(stacks) / e.SenderArgs.Receivers.Count;
            trait.Storage.Add(e.ReceiverField.GuidStr, null);
            await trait.AnimActivationShort();
            await owner.Strength.AdjustValueScale(effect, trait);
        }
    }
}
