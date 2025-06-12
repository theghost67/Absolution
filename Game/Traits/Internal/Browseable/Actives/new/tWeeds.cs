using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWeeds : ActiveTrait
    {
        const string ID = "weeds";
        const int CD = 1;
        const string KEY = "cd";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);

        public tWeeds() : base(ID)
        {
            name = "Целебные травы";
            desc = "Звали доктора?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerTriple);
        }
        protected tWeeds(tWeeds other) : base(other) { }
        public override object Clone() => new tWeeds(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на союзном поле рядом, включая владельца</color>\n" +
                   $"Удаляет все навыки на цели (сначала пассивные) за исключением навыка <nobr><u>{name}</u></nobr>. Перезарядка: {CD} х.\n\n" +
                   $"<color>В начале хода на территории</color>\nЕсли на прошлом ходу навык не был на перезарядке, " +
                   $"восстанавливает {_healthF.Format(args.stacks)} здоровья союзным картам рядом, исключая владельца.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;

            ITableTraitList list = target.Card.Traits.Passives;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ITableTraitListElement element = list[i];
                await list.AdjustStacks(element.Trait.Data.id, -element.Stacks, trait, null);
            }

            list = target.Card.Traits.Actives;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ITableTraitListElement element = list[i];
                if (element.Trait.Data.id == ID) continue;
                await list.AdjustStacks(element.Trait.Data.id, -element.Stacks, trait, null);
            }

            trait.SetCooldown(CD);
            trait.Storage[KEY] = CD;
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard owner = trait?.Owner;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || owner.IsKilled) return;

            if (trait.Storage.ContainsKey(KEY))
            {
                trait.Storage.Remove(KEY);
                return;
            }

            BattleFieldCard[] cards = terr.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            await trait.AnimActivation();
            int health = _healthF.ValueInt(trait.GetStacks());
            foreach (BattleFieldCard card in cards)
                await card.Health.AdjustValue(health, trait);
        }
    }
}
