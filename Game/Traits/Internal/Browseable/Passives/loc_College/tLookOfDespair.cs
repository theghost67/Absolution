using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLookOfDespair : PassiveTrait
    {
        const string ID = "look_of_despair";
        const int PRIORITY = 7;
        static readonly TraitStatFormula _moxieF = new(false, 0, 5);

        public tLookOfDespair() : base(ID)
        {
            name = "Взгляд отчаяния";
            desc = "Я видел некоторое дерьмо...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tLookOfDespair(tLookOfDespair other) : base(other) { }
        public override object Clone() => new tLookOfDespair(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении карты напротив владельца (П{PRIORITY})</color>\nУменьшает её инициативу на {_moxieF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(32, stacks);
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            string entryId = $"{trait.Guid}/{e.target.Guid}";

            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await e.target.Moxie.AdjustValue(-_moxieF.Value(e.traitStacks), trait, entryId);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await e.target.Moxie.RevertValue(entryId);
            }
        }
    }
}
