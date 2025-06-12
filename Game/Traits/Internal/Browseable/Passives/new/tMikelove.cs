using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMikelove : PassiveTrait
    {
        const string ID = "mikelove";
        static readonly TraitStatFormula _statsF = new(true, 1.00f, 0.00f);

        public tMikelove() : base(ID)
        {
            name = "Майклав";
            desc = "Вступайте в пирамиду Майкла - это <i>беспроигрышная стратегия!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tMikelove(tMikelove other) : base(other) { }
        public override object Clone() => new tMikelove(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При установке владельца в качестве пятой карты с навыком {name} на одной стороне</color>\n" +
                   $"Даёт всем картам +{_statsF.Format(args.stacks)} к здоровью и силе. Тратит все заряды навыка {name} у всех союзных карт.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Side.Fields().WithCard().Select(f => f.Card).Where(c => c.Traits.Passive(ID) != null).ToArray();
            if (cards.Length != BattleTerritory.MAX_WIDTH) return;

            await trait.AnimActivation();
            float stats = _statsF.Value(trait.GetStacks());
            foreach (BattleFieldCard card in cards)
            {
                await card.Health.AdjustValueScale(stats, trait);
                await card.Strength.AdjustValueScale(stats, trait);
                await card.Traits.Passives.SetStacks(ID, 0, trait);
            }
        }
    }
}
