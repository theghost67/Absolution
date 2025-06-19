using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tUnpleasantScent : PassiveTrait
    {
        const string ID = "unpleasant_scent";
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tUnpleasantScent() : base(ID)
        {
            name = Translator.GetString("trait_unpleasant_scent_1");
            desc = Translator.GetString("trait_unpleasant_scent_2");

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tUnpleasantScent(tUnpleasantScent other) : base(other) { }
        public override object Clone() => new tUnpleasantScent(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_unpleasant_scent_3", _moxieF.Format(args.stacks, true));
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (killer == null) return;

            await trait.AnimActivation();
            await killer.Moxie.AdjustValue(-_moxieF.Value(trait.GetStacks()), trait);
        }
    }
}
