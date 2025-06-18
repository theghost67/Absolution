using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHammerOut : ActiveTrait
    {
        const string ID = "hammer_out";
        const int CD = 1;
        static readonly TraitStatFormula _damageF = new(false, 0, 2);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tHammerOut() : base(ID)
        {
            name = Translator.GetString("trait_hammer_out_1");
            desc = Translator.GetString("trait_hammer_out_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tHammerOut(tHammerOut other) : base(other) { }
        public override object Clone() => new tHammerOut(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_hammer_out_3", _damageF.Format(args.stacks), _moxieF.Format(args.stacks, true), CD);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            BattleFieldCard owner = trait.Owner;
            trait.SetCooldown(CD);

            int damage = _damageF.ValueInt(e.traitStacks);
            int moxie = _moxieF.ValueInt(e.traitStacks);
            target.Drawer?.CreateTextAsDamage(damage, false);
            await target.Health.AdjustValue(-damage, trait);
            if (!target.IsKilled)
                await target.Moxie.AdjustValue(-moxie, trait);
        }
    }
}
