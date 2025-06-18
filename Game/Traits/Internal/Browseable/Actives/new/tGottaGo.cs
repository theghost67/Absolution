using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tGottaGo : ActiveTrait
    {
        const string ID = "gotta_go";

        public tGottaGo() : base(ID)
        {
            name = "Gotta go fast!";
            desc = Translator.GetString("trait_gotta_go_1");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tGottaGo(tGottaGo other) : base(other) { }
        public override object Clone() => new tGottaGo(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_gotta_go_2");

        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null && GetFarthestCard((IBattleTrait)e.trait, (BattleField)e.target) != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            BattleFieldCard farthestCard = GetFarthestCard(trait, target);
            await farthestCard.TryAttachToField(target, trait);
        }

        BattleFieldCard GetFarthestCard(IBattleTrait trait, BattleField target)
        {
            BattleFieldCard card = null;
            if (target.pos.x < trait.Owner.Field.pos.x)
            {
                for (int i = target.pos.x - 1; i > 0; i--)
                {
                    card = trait.Territory.Field(i, trait.Field.pos.y).Card;
                    if (card != null) break;
                }
            }
            else
            {
                for (int i = target.pos.x + 1; i < BattleTerritory.MAX_WIDTH; i++)
                {
                    card = trait.Territory.Field(i, trait.Field.pos.y).Card;
                    if (card != null) break;
                }
            }
            return card;
        }
    }
}
