using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStealth : PassiveTrait
    {
        const string ID = "stealth";
        const string TRAIT_ID = "evasion";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);

        public tStealth() : base(ID)
        {
            name = "Стелс";
            desc = "Стелс.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tStealth(tStealth other) : base(other) { }
        public override object Clone() => new tStealth(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После убийства любой карты владельцем с одной атаки</color>\n" +
                   $"накладывает на владельца навык <nobr><u>{traitName}</u></nobr>.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = _stacksF.ValueInt(args.stacks) } };
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed);
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
            }
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard receiver = e.ReceiverCard;
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (receiver == null || receiver.IsKilled) return;
            trait.Storage.TryAdd(receiver.GuidStr, null);
        }
        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Storage.ContainsKey(e.victim.GuidStr)) return;
            await trait.AnimActivation();
            await owner.Traits.AdjustStacks(TRAIT_ID, trait.GetStacks(), trait);
        }
    }
}
