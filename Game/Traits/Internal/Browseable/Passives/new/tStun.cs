using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStun : PassiveTrait
    {
        const string ID = "stun";

        public tStun() : base(ID)
        {
            name = Translator.GetString("trait_stun_1");
            desc = Translator.GetString("trait_stun_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tStun(tStun other) : base(other) { }
        public override object Clone() => new tStun(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_stun_3");
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled) return;

            await trait.AnimActivation();
            e.handled = true;
            await trait.SetStacks(0, trait);
        }
    }
}
