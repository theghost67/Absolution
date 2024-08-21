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
        static readonly TraitStatFormula _moxieF = new(false, 0, 2);
        static readonly TraitStatFormula _healthIncF = new(true, 0.25f, 0.25f);
        static readonly TraitStatFormula _strengthIncF = new(true, 0.25f, 0.25f);

        public tTimeToDecide() : base(ID)
        {
            name = "Время решать";
            desc = "Не делай этого, Коннор.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tTimeToDecide(tTimeToDecide other) : base(other) { }
        public override object Clone() => new tTimeToDecide(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При использовании на союзном поле</color>\nСтановится девиантом, получая {_moxieF.Format(args.stacks)} инициативы или вдвое больше, если на союзных полях есть карты с таким же навыком. Тратит все заряды.\n\n" +
                   $"<color>При использовании на вражеском поле</color>\nОстаётся машиной, получая бонус в {_healthIncF.Format(args.stacks)} к здоровью и {_strengthIncF.Format(args.stacks)} к силе. Даёт владельцу навык <u>{traitName}</u>. Тратит все заряды.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(20, stacks);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            bool isAllyField = e.target.IsMine();
            int stacks = e.traitStacks;

            await trait.SetStacks(0, trait.Side);
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
        }
    }
}
