using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExoskeleton : PassiveTrait
    {
        const string ID = "exoskeleton";
        static readonly TraitStatFormula _valueF = new(false, 0, 2);

        public tExoskeleton() : base(ID)
        {
            name = "Экзоскелет";
            desc = "Эластичный живой биоматериал поверх титанового экзоскелета.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tExoskeleton(tExoskeleton other) : base(other) { }
        public override object Clone() => new tExoskeleton(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При получении любого урона или лечения</color>\nСнижает получаемый урон или лечение на {_valueF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.5f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Health.OnPreSet.Add(trait.GuidStr, OnStatPreSet);
            else if (trait.WasRemoved(e))
                trait.Owner.Health.OnPreSet.Remove(trait.GuidStr);
        }
        static async UniTask OnStatPreSet(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleFieldCard owner = (BattleFieldCard)stat.Owner;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.deltaValue == 0) return;

            int value = _valueF.ValueInt(trait.GetStacks());
            await trait.AnimActivationShort();
            if (e.deltaValue > 0)
                 e.deltaValue = Mathf.Max(0, e.deltaValue - value);
            else e.deltaValue = Mathf.Min(0, e.deltaValue + value);
        }
    }
}
