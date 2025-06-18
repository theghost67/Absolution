using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAdrenaline : PassiveTrait
    {
        const string ID = "adrenaline";

        public tAdrenaline() : base(ID)
        {
            name = Translator.GetString("trait_adrenaline_1");
            desc = Translator.GetString("trait_adrenaline_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tAdrenaline(tAdrenaline other) : base(other) { }
        public override object Clone() => new tAdrenaline(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_adrenaline_3");

        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPreKilled.Add(trait.GuidStr, OnOwnerPreKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPreKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPreKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            await owner.Health.SetValue(1, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
