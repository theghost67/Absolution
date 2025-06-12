using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTea : ActiveTrait
    {
        const string ID = "tea";
        static readonly TraitStatFormula _stacksPerDeathF = new(false, 1, 0);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);
        static readonly TraitStatFormula _strengthF = new(true, 0.50f, 0);

        public tTea() : base(ID)
        {
            name = "Хочешь чаю?";
            desc = "Уже наливаю! :)";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tTea(tTea other) : base(other) { }
        public override object Clone() => new tTea(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После смерти любой вражеской карты</color>\nДаёт {_stacksPerDeathF.Format(args.stacks)} зарядов навыка.\n\n" +
                   $"<color>При активации на вражеской карте рядом</color>\nУменьшает инициативу цели на {_moxieF.Format(args.stacks)} и " +
                   $"её силу на {_strengthF.Format(args.stacks, true)}. Тратит один заряд. Активация возможна только при наличии двух и более зарядов.";
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.traitStacks > 1;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            await target.Card.Moxie.AdjustValue(-_moxieF.ValueInt(e.traitStacks), trait);
            await target.Card.Strength.AdjustValueScale(-_strengthF.Value(e.traitStacks), trait);
            await trait.AdjustStacks(-1, owner.Side);
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

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Add(trait.GuidStr, OnPostKilled);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            card.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            BattleTerritory terr = card.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || card.Side == trait.Side) return;
            await trait.AdjustStacks(1, trait);
        }
    }
}
