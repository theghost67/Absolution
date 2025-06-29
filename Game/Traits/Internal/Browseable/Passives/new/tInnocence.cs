using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tInnocence : PassiveTrait
    {
        const string ID = "innocence";
        const string TRAIT_ID = "bad_karma";

        public tInnocence() : base(ID)
        {
            name = Translator.GetString("trait_innocence_1");
            desc = Translator.GetString("trait_innocence_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tInnocence(tInnocence other) : base(other) { }
        public override object Clone() => new tInnocence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_innocence_3", traitName);
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
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
            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (trait == null || trait.Owner == null || killer == null) return;

            await trait.AnimActivation();
            killer.Data.traits.Passives.AdjustStacks(TRAIT_ID, 1);
            await killer.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
