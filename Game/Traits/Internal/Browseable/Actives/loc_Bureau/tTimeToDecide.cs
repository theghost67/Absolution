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
        static readonly TraitStatFormula _healthIncF = new(true, 0.00f, 0.50f);
        static readonly TraitStatFormula _strengthIncF = new(true, 0.00f, 0.50f);

        public tTimeToDecide() : base(ID)
        {
            name = "Время решать, Коннор";
            desc = "Не делай этого, Коннор.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothSingle);
        }
        protected tTimeToDecide(tTimeToDecide other) : base(other) { }
        public override object Clone() => new tTimeToDecide(this);

        public override string DescRich(ITableTrait trait)
        {
            int stacks = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на союзном поле",
                    $"Становится девиантом, получая {_moxieF.Format(trait, stacks)} инициативы или вдвое больше, если на союзных полях есть карты с таким же навыком. Тратит все заряды."),
                new($"При использовании на вражеском поле",
                    $"Остаётся машиной, получая бонус в {_healthIncF.Format(trait, stacks)} к здоровью и {_strengthIncF.Format(trait, stacks)} к силе. Вероятны негативные последствия. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(160, stacks);
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
            int stacks = trait.GetStacks();

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

                int moxie = _moxieF.ValueInt(trait, stacks);
                if (alliesHaveThisTrait)
                    moxie *= 2;
                await owner.Moxie.AdjustValue(moxie, trait);
            }
            else
            {
                await owner.Health.AdjustValueScale(_healthIncF.Value(trait, stacks), trait);
                await owner.Strength.AdjustValueScale(_strengthIncF.Value(trait, stacks), trait);
                await owner.Traits.AdjustStacks(TRAIT_ID, 1, trait);
            }
        }
    }
}
