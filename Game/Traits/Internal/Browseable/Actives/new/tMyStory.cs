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
    public class tMyStory : ActiveTrait
    {
        const string ID = "my_story";
        const int CD = 99;

        public tMyStory() : base(ID)
        {
            name = Translator.GetString("trait_my_story_1");
            desc = Translator.GetString("trait_my_story_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tMyStory(tMyStory other) : base(other) { }
        public override object Clone() => new tMyStory(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_my_story_3", CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
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
            owner.Health.OnPreSet.Add(trait.GuidStr, OnOwnerHealthPreSet);
            owner.Traits.Passives.OnStacksTryToChange.Add(trait.GuidStr, OnOwnerStacksTryToChange);
            owner.Traits.Actives.OnStacksTryToChange.Add(trait.GuidStr, OnOwnerStacksTryToChange);
            owner.Territory.OnStartPhase.Add(trait.GuidStr, OnOwnerTerritoryStartPhase);
            trait.SetCooldown(CD);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            if (trait.WasRemoved(e))
            {
                owner.Health.OnPreSet.Remove(trait.GuidStr);
                owner.Traits.Passives.OnStacksTryToChange.Remove(trait.GuidStr);
                owner.Traits.Actives.OnStacksTryToChange.Remove(trait.GuidStr);
                owner.Territory.OnStartPhase.Remove(trait.GuidStr);
            }
        }

        private async UniTask OnOwnerHealthPreSet(object sender, TableStat.PreSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleFieldCard owner = (BattleFieldCard)stat.Owner;
            IBattleTrait trait = owner.Traits.Active(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.deltaValue >= 0) return;
            e.handled = true;
            await trait.AnimActivationShort();
        }
        private async UniTask OnOwnerStacksTryToChange(object sender, TableTraitStacksTryArgs e)
        {
            IBattleTraitList list = (IBattleTraitList)sender;
            BattleFieldCard owner = list.Set.Owner;
            IBattleTrait trait = owner.Traits.Active(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.source.AsBattleFieldCard() == owner) return;
            e.handled = true;
            await trait.AnimActivationShort();
        }
        private async UniTask OnOwnerTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard owner = trait.Owner;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            await trait.SetStacks(0, trait);
        }
    }
}
