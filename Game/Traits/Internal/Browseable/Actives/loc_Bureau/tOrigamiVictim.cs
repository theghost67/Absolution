using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrigamiVictim : ActiveTrait
    {
        const string ID = "origami_victim";
        const string TRAIT_ID = "origami_mark";
        const int CD = 1;

        public tOrigamiVictim() : base(ID)
        {
            name = "Жертва Мастера Оригами";
            desc = "ШООООООООООООН!";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tOrigamiVictim(tOrigamiVictim other) : base(other) { }
        public override object Clone() => new tOrigamiVictim(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой вражеской карте",
                    $"Объявляет её следующей жертвой Мастера Оригами и накладывает навык <i>{traitName}</i>. Перезарядка: {CD} х."),
            });
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            trait.Storage.turnsDelay += CD;
            await target.Traits.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
