using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tReflection : PassiveTrait
    {
        const string ID = "reflection";
        static readonly TraitStatFormula _strengthF = new(true, 0.75f, 0.25f);

        public tReflection() : base(ID)
        {
            name = Translator.GetString("trait_reflection_1");
            desc = Translator.GetString("trait_reflection_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tReflection(tReflection other) : base(other) { }
        public override object Clone() => new tReflection(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_reflection_3", _strengthF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.5f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            if (owner.IsKilled) return;
            if (e.Sender.IsKilled || e.Sender.Field == null) return;

            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled) return;

            await trait.AnimActivationShort();
            int strength = (int)Mathf.Ceil(e.Strength * _strengthF.Value(trait.GetStacks()));
            BattleInitiationSendArgs initiation = new(owner, strength, true, false, e.Sender.Field);
            await owner.Territory.Initiations.EnqueueAndAwait(initiation);
        }
    }
}
