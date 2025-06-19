using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStalker : PassiveTrait
    {
        const string ID = "stalker";
        static readonly TraitStatFormula _strengthF = new(true, 0.20f, 0.10f);
        int _turn = -1;

        public tStalker() : base(ID)
        {
            name = Translator.GetString("trait_stalker_1");
            desc = "...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tStalker(tStalker other) : base(other) { _turn = other._turn; }
        public override object Clone() => new tStalker(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_stalker_2", _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(10, stacks, 1, 1.75f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
            }
        }

        async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || e.victim.Field == null) return;
            if (trait.TurnAge == _turn) return;
            if (trait.Storage.ContainsKey(e.victim.Field.GuidStr)) return;

            _turn = trait.TurnAge;
            int stacks = trait.GetStacks();
            float effect = _strengthF.Value(stacks);
            trait.Storage.Add(e.victim.Field.GuidStr, null);
            await trait.AnimActivationShort();
            await owner.Strength.AdjustValueScale(effect, trait);
        }
        async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || e.ReceiverCard == null) return;
            if (trait.TurnAge == _turn) return;
            if (trait.Storage.ContainsKey(e.ReceiverField.GuidStr)) return;

            _turn = trait.TurnAge;
            int stacks = trait.GetStacks();
            float effect = _strengthF.Value(stacks);
            trait.Storage.Add(e.ReceiverField.GuidStr, null);
            await trait.AnimActivationShort();
            await owner.Strength.AdjustValueScale(effect, trait);
        }
    }
}
