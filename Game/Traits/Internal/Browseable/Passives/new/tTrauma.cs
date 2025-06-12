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
    public class tTrauma : PassiveTrait
    {
        const string ID = "trauma";

        public tTrauma() : base(ID)
        {
            name = "Травма";
            desc = "Беги! Верно... ты же не можешь. Хе-хе.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tTrauma(tTrauma other) : base(other) { }
        public override object Clone() => new tTrauma(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Пока присутствует</color>\nБлокирует возможность лечения владельца любыми источниками.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Health.OnPreSet.Add(trait.GuidStr, OnHealthPreSet);
            else if (trait.WasRemoved(e))
                trait.Owner.Health.OnPreSet.Remove(trait.GuidStr);
        }
        static async UniTask OnHealthPreSet(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleFieldCard owner = (BattleFieldCard)stat.Owner;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.deltaValue <= 0) return;

            e.handled = true;
            await trait.AnimActivationShort();
        }
    }
}
