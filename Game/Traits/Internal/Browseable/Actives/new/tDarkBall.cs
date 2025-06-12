using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDarkBall : ActiveTrait
    {
        const string ID = "dark_ball";
        static readonly TraitStatFormula _chargesPerDeathF = new(false, 1, 0);
        static readonly TraitStatFormula _directDamageF = new(false, 0, 4);
        static readonly TraitStatFormula _splashDamageF = new(false, 0, 2);
        static readonly TerritoryRange _chargesRange = TerritoryRange.ownerDouble;

        public tDarkBall() : base(ID)
        {
            name = "Тёмный шар";
            desc = "Дочь тьмы уничтожит всех своими тёмными шарами.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tDarkBall(tDarkBall other) : base(other) { }
        public override object Clone() => new tDarkBall(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на поле рядом напротив</color>\nНаносит {_directDamageF.Format(args.stacks)} урона цели и " +
                   $"{_splashDamageF.Format(args.stacks)} соседним от цели картам. Получает {_chargesPerDeathF.Format(args.stacks)} зарядов после смерти " +
                   $"карты рядом с владельцем. Этот навык можно использовать только при более, чем одном заряде.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _directDamageF.Value(result.Entity.GetStacks()), 0);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.ContinuousAttachHandler_Add(trait.GuidStr, ContinuousAttach_Add);
            else if (trait.WasRemoved(e))
                trait.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.traitStacks > 1;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            int directDamage = _directDamageF.ValueInt(e.traitStacks);
            int splashDamage = _splashDamageF.ValueInt(e.traitStacks);

            target.Drawer?.CreateTextAsDamage(directDamage, false);
            if (target.Card != null)
                 await target.Card.Health.AdjustValue(-directDamage, trait);
            else await target.Health.AdjustValue(-directDamage, trait);

            BattleFieldCard[] splashCards = owner.Territory.Fields(target.pos, _chargesRange).WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard card in splashCards)
            {
                card.Drawer?.CreateTextAsDamage(directDamage, false);
                await card.Health.AdjustValue(-directDamage, trait);
            }

            await trait.AdjustStacks(-1, owner.Side);
        }

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Add(trait.GuidStr, OnCardPostKilled);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnCardPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            BattleActiveTrait trait = (BattleActiveTrait)TraitFinder.FindInBattle(card.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.IsKilled || trait.Field == null) return;

            bool isInRange = trait.Territory.Fields(trait.Field.pos, _chargesRange).Contains(card.Field);
            if (!isInRange) return;

            await trait.AdjustStacks(_chargesPerDeathF.ValueInt(trait.GetStacks()), trait);
        }
    }
}
