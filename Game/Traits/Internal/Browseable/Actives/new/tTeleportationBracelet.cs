using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTeleportationBracelet : ActiveTrait
    {
        const string ID = "teleportation_bracelet";
        const int CD = 2;
        static readonly TraitStatFormula _statsF = new(false, 1, 0);

        public tTeleportationBracelet() : base(ID)
        {
            name = Translator.GetString("trait_teleportation_bracelet_1");
            desc = Translator.GetString("trait_teleportation_bracelet_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tTeleportationBracelet(tTeleportationBracelet other) : base(other) { }
        public override object Clone() => new tTeleportationBracelet(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_teleportation_bracelet_3", _statsF.Format(args.stacks), CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            int stats = _statsF.ValueInt(e.traitStacks);
            await owner.TryAttachToField(target, trait);
            await owner.Price.AdjustValue(stats, trait);
            await owner.Moxie.AdjustValue(stats, trait);
            trait.SetCooldown(CD);
        }
    }
}
