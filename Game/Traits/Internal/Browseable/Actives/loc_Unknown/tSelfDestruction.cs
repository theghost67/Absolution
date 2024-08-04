using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSelfDestruction : ActiveTrait
    {
        const string ID = "self_destruction";

        public tSelfDestruction() : base(ID)
        {
            name = "Самоуничтожение";
            desc = "Нет, не трогай кнопку самоуничтожения!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tSelfDestruction(tSelfDestruction other) : base(other) { }
        public override object Clone() => new tSelfDestruction(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"Убивает владельца."),
            });
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            await owner.TryKill(BattleKillMode.Default, trait);
        }
    }
}
