using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFry : PassiveTrait
    {
        const string ID = "fry";
        const string TRAIT_ID = "flame";
        static readonly TraitStatFormula _stacksF = new(true, 0.125f, 0.125f);

        public tFry() : base(ID)
        {
            name = Translator.GetString("trait_fry_1");
            desc = Translator.GetString("trait_fry_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tFry(tFry other) : base(other) { }
        public override object Clone() => new tFry(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_fry_3", traitName, _stacksF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(20, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnInitiationConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }
        static async UniTask OnInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.ReceiverCard == null || e.ReceiverCard.IsKilled || e.ReceiverCard.Traits.Passive(TRAIT_ID) != null) return;

            int stacks = (int)Math.Ceiling(e.Strength * _stacksF.Value(trait.GetStacks()));
            await trait.AnimActivation();
            await e.ReceiverCard.Traits.Passives.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
