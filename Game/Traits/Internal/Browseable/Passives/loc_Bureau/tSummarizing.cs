using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSummarizing : PassiveTrait
    {
        const string ID = "summarizing";
        const int PRIORITY = 5;
        const string TRAIT_ID = "sentence";
        static readonly TraitStatFormula _ratioF = new(true, 0.50f, 0.50f);

        public tSummarizing() : base(ID)
        {
            name = "Резюмирование";
            desc = "Приговор может быть смертельнее всякого оружия.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tSummarizing(tSummarizing other) : base(other) { }
        public override object Clone() => new tSummarizing(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После атаки на владельца (П{PRIORITY})</color>\n" +
                   $"Увеличивает количество зарядов навыка <nobr><color><u>{traitName}</u></color></nobr> на {_ratioF.Format(args.stacks)} от силы атаки.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(10, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            float ratio = _ratioF.Value(trait.GetStacks());
            int stacks = (e.Strength * ratio).Ceiling();
            await owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
