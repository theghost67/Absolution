using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrigamiVictim : ActiveTrait
    {
        const string ID = "origami_victim";
        const string TRAIT_ID = "origami_mark";
        const int CD = 1;

        public tOrigamiVictim() : base(ID)
        {
            name = Translator.GetString("trait_origami_victim_1");
            desc = Translator.GetString("trait_origami_victim_2");

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tOrigamiVictim(tOrigamiVictim other) : base(other) { }
        public override object Clone() => new tOrigamiVictim(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_origami_victim_3", traitName, CD);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            trait.SetCooldown(CD);
            await target.Traits.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
