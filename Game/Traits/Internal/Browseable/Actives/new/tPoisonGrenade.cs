using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPoisonGrenade : ActiveTrait
    {
        const string ID = "poison_grenade";
        const int CD = 1;
        const string TRAIT_ID = "poison";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);

        public tPoisonGrenade() : base(ID)
        {
            name = "Чумная граната";
            desc = "Медленная, мучительная смерть.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tPoisonGrenade(tPoisonGrenade other) : base(other) { }
        public override object Clone() => new tPoisonGrenade(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на вражеском поле рядом</color>\n" +
                   $"Наносит рядомстоящим от цели картам, включая цель, навык <u><color>{traitName}</color></u> с {_stacksF.Format(args.stacks)} зарядов. " +
                   $"Перезарядка: {CD} х.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = args.stacks } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && 
                e.trait.Territory.Fields(e.target.pos, TerritoryRange.ownerTriple).WithCard().ToArray().Length != 0;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard[] targets = trait.Territory.Fields(target.pos, TerritoryRange.ownerTriple).WithCard().Select(f => f.Card).ToArray();
            int stacks = _stacksF.ValueInt(e.traitStacks);
            foreach (BattleFieldCard card in targets)
                await card.Traits.Passives.AdjustStacks(TRAIT_ID, stacks, trait);
            trait.SetCooldown(CD);
        }
    }
}
