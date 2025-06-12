using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMiracleDrug : PassiveTrait
    {
        const string ID = "miracle_drug";
        const string TRAIT_ID = "miracle_aftertaste";

        public tMiracleDrug() : base(ID)
        {
            name = "Чудодейственный препарат";
            desc = "Я посоветовал ему чудодейственный вегетерианский салат, который ты, инвалид, схомячил!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tMiracleDrug(tMiracleDrug other) : base(other) { }
        public override object Clone() => new tMiracleDrug(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После убийства владельца</color>\nНакладывает на убийцу навык <nobr><color><u>{traitName}</u></color></nobr>.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { stacks = args.stacks, linkFormat = true } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }
        static async UniTask OnPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || killer == null) return;

            await trait.AnimActivation();
            await killer.Traits.Passives.AdjustStacks(TRAIT_ID, trait.GetStacks(), trait);
        }
    }
}
