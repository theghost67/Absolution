using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHeroesNeverDie : PassiveTrait
    {
        const string ID = "heroes_never_die";

        public tHeroesNeverDie() : base(ID)
        {
            name = "Герои не умирают";
            desc = "Она танка воскресила...";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tHeroesNeverDie(tHeroesNeverDie other) : base(other) { }
        public override object Clone() => new tHeroesNeverDie(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>На пороге смерти стороны-владельца</color>\n" +
                   $"Если владелец находится на территории, жертвует владельцем, передавая всё его здоровье стороне-владельцу. Тратит все заряды.";
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Side.OnDeathsDoor.Add(trait.GuidStr, OnSideDeathsDoor);
            else if (trait.WasRemoved(e))
                trait.Side.OnDeathsDoor.Remove(trait.GuidStr);
        }

        async UniTask OnSideDeathsDoor(object sender, BattleKillAttemptArgs e)
        {
            BattleSide side = (BattleSide)sender;
            BattleTerritory terr = side.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            await trait.AnimActivation();
            await owner.Side.Health.AdjustValue(owner.Health, trait);
            await owner.Health.SetValue(0, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
