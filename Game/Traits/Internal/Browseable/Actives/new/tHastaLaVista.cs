using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHastaLaVista : ActiveTrait
    {
        const string ID = "hasta_la_vista";
        static readonly TraitStatFormula _damageF = new(false, 0, 4);

        public tHastaLaVista() : base(ID)
        {
            name = Translator.GetString("trait_hasta_la_vista_1");
            desc = Translator.GetString("trait_hasta_la_vista_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tHastaLaVista(tHastaLaVista other) : base(other) { }
        public override object Clone() => new tHastaLaVista(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_hasta_la_vista_3", _damageF.Format(args.stacks));

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _damageF.Value(result.Entity.GetStacks()) * 2);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(10, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleField target = owner.Field.Opposite;

            int strength = _damageF.ValueInt(e.traitStacks);
            BattleInitiationSendArgs initiation = new(owner, strength, true, false, target);

            await owner.Territory.Initiations.EnqueueAndAwait(initiation);

            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, TerritoryRange.ownerDouble).WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard card in cards)
                await card.TryAttachToSideSleeve(owner.Side, trait);

            await owner.TryAttachToSideSleeve(owner.Side, trait);
            await trait.SetStacks(0, owner.Side);
        }
    }
}
