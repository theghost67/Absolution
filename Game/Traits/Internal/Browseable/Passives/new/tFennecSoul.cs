using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFennecSoul : PassiveTrait
    {
        const string ID = "fennec_soul";

        public tFennecSoul() : base(ID)
        {
            name = "Феннекийская душа";
            desc = "Учился у худших.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tFennecSoul(tFennecSoul other) : base(other) { }
        public override object Clone() => new tFennecSoul(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед получением атаки владельцем</color>\nПеренаправляет атаку на поле, принадлежащее владельцу.\n\n" +
                   $"<color>В начале хода на территории</color>\nТратит все заряды.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
            }
        }

        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;
            await trait.SetStacks(0, trait);
        }
        private async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (owner.IsKilled || trait == null) return;
            await trait.AnimActivationShort();
            e.IgnoresCard = true;
        }
    }
}
