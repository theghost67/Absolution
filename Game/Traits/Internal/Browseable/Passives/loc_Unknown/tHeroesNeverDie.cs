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
        const int PRIORITY = 8;
        static readonly TraitStatFormula _strengthF = new(true, 1.00f, 0);

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

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"На пороге смерти стороны-владельца (П{PRIORITY})",
                    $"Если находится на территории, жертвует владельцем, передавая всё его здоровье стороне. Тратит все заряды."),
                new($"Постоянный эффект в бою",
                    $"Уменьшает силу на {_strengthF.Format(trait)}."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                string guid = trait.GuidGen(trait.Owner.Guid);
                trait.Side.OnDeathsDoor.Add(trait.GuidStr, OnSideDeathsDoor, PRIORITY);
                await trait.Owner.Strength.AdjustValueScale(-_strengthF.Value(trait), trait, guid);
            }
            else if (trait.WasRemoved(e))
            {
                string guid = trait.GuidGen(trait.Owner.Guid);
                trait.Side.OnDeathsDoor.Remove(trait.GuidStr);
                await trait.Owner.Strength.RevertValueScale(guid);
            }
        }

        async UniTask OnSideDeathsDoor(object sender, BattleKillAttemptArgs e)
        {
            BattleSide side = (BattleSide)sender;
            BattleTerritory terr = side.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            await trait.AnimActivation();
            await trait.SetStacks(0, trait);
            await owner.Side.Health.AdjustValue(owner.Health, trait);
            await owner.Health.SetValue(0, trait);
        }
    }
}
