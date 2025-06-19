using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTimeToDecide : ActiveTrait
    {
        const string ID = "time_to_decide";
        const string TRAIT_ID = "time_to_decide_machine";
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);
        static readonly TraitStatFormula _healthIncF = new(true, 1.00f, 0.00f);
        static readonly TraitStatFormula _strengthIncF = new(true, 1.00f, 0.00f);

        public tTimeToDecide() : base(ID)
        {
            name = Translator.GetString("trait_time_to_decide_1");
            desc = Translator.GetString("trait_time_to_decide_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tTimeToDecide(tTimeToDecide other) : base(other) { }
        public override object Clone() => new tTimeToDecide(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_time_to_decide_3", _moxieF.Format(args.stacks), _healthIncF.Format(args.stacks), _strengthIncF.Format(args.stacks), traitName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            bool isAllyField = e.target.IsMine();
            int stacks = e.traitStacks;

            if (isAllyField)
            {
                bool alliesHaveThisTrait = false;
                IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.ownerAllNotSelf).WithCard();
                foreach (BattleField field in fields)
                {
                    if (field.Card.Traits.Any(ID) == null) continue;
                    alliesHaveThisTrait = true;
                    break;
                }

                int moxie = _moxieF.ValueInt(stacks);
                if (alliesHaveThisTrait)
                    moxie *= 2;
                await owner.Moxie.AdjustValue(moxie, trait);
            }
            else
            {
                await owner.Health.AdjustValueScale(_healthIncF.Value(stacks), trait);
                await owner.Strength.AdjustValueScale(_strengthIncF.Value(stacks), trait);
                await owner.Traits.AdjustStacks(TRAIT_ID, 1, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
