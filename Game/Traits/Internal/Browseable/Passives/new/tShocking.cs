using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tShocking : PassiveTrait
    {
        const string ID = "shocking";
        const string TRAIT_ID = "shock";

        public tShocking() : base(ID)
        {
            name = "Шокирующий";
            desc = "Ы-хы-хы-хы!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tShocking(tShocking other) : base(other) { }
        public override object Clone() => new tShocking(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После совершения атаки владельцем</color>\nНакладывает на цель навык <nobr><u>{traitName}</u></nobr>, если он у неё отсутствует.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
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
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattleFieldCard target = e.ReceiverCard;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || target == null || target.IsKilled || target.Traits.Passive(TRAIT_ID) != null) return;

            await trait.AnimActivation();
            await target.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
