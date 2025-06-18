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
            name = Translator.GetString("trait_self_destruction_1");
            desc = Translator.GetString("trait_self_destruction_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tSelfDestruction(tSelfDestruction other) : base(other) { }
        public override object Clone() => new tSelfDestruction(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_self_destruction_3");
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            await owner.TryKill(BattleKillMode.Default, trait);
        }
    }
}
