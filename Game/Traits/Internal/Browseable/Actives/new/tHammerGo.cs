using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHammerGo : ActiveTrait
    {
        const string ID = "hammer_go";
        const int CD = 3;
        const string TRAIT_ID = "stun";
        static readonly TraitStatFormula _damageF = new(true, 0.25f, 0.25f);

        public tHammerGo() : base(ID)
        {
            name = Translator.GetString("trait_hammer_go_1");
            desc = "For the Crusaders!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tHammerGo(tHammerGo other) : base(other) { }
        public override object Clone() => new tHammerGo(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_hammer_go_2", _damageF.Format(args.stacks), traitName, CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            float strengthThreshold = result.Entity.Owner.Strength * _damageF.Value(result.Entity.GetStacks()) * 0.75f;
            return new(result.Entity, strengthThreshold);
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(16, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            trait.SetCooldown(CD);

            int damage = (int)Mathf.Ceil(_damageF.Value(e.traitStacks) * owner.Strength);
            target?.Drawer.CreateTextAsDamage(damage, false);
            if (target.Card != null)
                 await target.Card.Health.AdjustValue(-damage, trait);
            else await target.Health.AdjustValue(-damage, trait);

            BattleFieldCard[] cards = owner.Territory.Fields(target.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard card in cards)
                await card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
