using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExposed : PassiveTrait
    {
        const string ID = "exposed";

        public tExposed() : base(ID)
        {
            name = "Уязвимость";
            desc = "Куда подевался весь твой альтруизм, а?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tExposed(tExposed other) : base(other) { }
        public override object Clone() => new tExposed(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После получения атаки владельцем</color>\nУбивает владельца, даже если атака не нанесла урон.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnInitiationPostReceived);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;
            await trait.SetStacks(0, trait);
        }
        private async UniTask OnInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(owner.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled) return;
            await owner.TryKill(BattleKillMode.Default, trait);
        }
    }
}
