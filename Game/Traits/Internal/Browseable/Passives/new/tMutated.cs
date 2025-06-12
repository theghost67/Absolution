using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMutated : PassiveTrait
    {
        const string ID = "mutated";
        const string KEY = ID;
        const int MOXIE_PER_MISSING_HEALTH = 1;
        const int MISSING_HEALTH_PERCENT = 25;
        const float MISSING_HEALTH_RATIO = 0.25f;

        public tMutated() : base(ID)
        {
            name = "Мутированный";
            desc = "Как он вообще появился на свет? ...Не знаю, в пробирке вылупился, видимо.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tMutated(tMutated other) : base(other) { }
        public override object Clone() => new tMutated(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При уменьшении здоровья владельца</color>\nДаёт бонус к инициативе владельца в зависимости от его здоровья: " +
                   $"+{MOXIE_PER_MISSING_HEALTH} ед. за каждые {MISSING_HEALTH_PERCENT}% отсутствующего здоровья";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            trait.Storage[KEY] = 0;

            if (trait.WasAdded(e))
                trait.Owner.Health.OnPostSet.Add(trait.GuidStr, OnHealthPostSet);
            else if (trait.WasRemoved(e))
                trait.Owner.Health.OnPostSet.Remove(trait.GuidStr);
        }
        private async UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleFieldCard owner = (BattleFieldCard)stat.Owner;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int healthChunk = (int)Math.Ceiling(owner.Data.health * MISSING_HEALTH_RATIO);
            int prevBonus = (int)trait.Storage[KEY];
            int currBonus = (int)((1 / MISSING_HEALTH_RATIO) - (float)Math.Floor((float)owner.Health / healthChunk));
            if (currBonus < 0)
                currBonus = 0;
            if (prevBonus == currBonus) return;

            await trait.AnimActivationShort();
            await owner.Moxie.RevertValue(trait.GuidStr);
            await owner.Moxie.AdjustValue(currBonus, trait, trait.GuidStr);
            trait.Storage[KEY] = (int)owner.Moxie.EntryValue(trait.GuidStr);
        }
    }
}
