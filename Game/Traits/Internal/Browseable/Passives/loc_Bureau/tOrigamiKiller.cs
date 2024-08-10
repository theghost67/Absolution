using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrigamiKiller : PassiveTrait
    {
        const string ID = "origami_killer";
        const int PRIORITY = 6;
        const string CARD_ID = "origami";
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.3333f);

        public tOrigamiKiller() : base(ID)
        {
            name = "Мастер Оригами";
            desc = "Хорошо. Я Мастер Оригами. Я сажаю жертв в машину. Топлю в дождевой воде. Потом бросаю на пустыре с оригами, " +
                   "зажатой в кулаке, и орхидеей на груди. А всё потому, что мне скучно, мистер Шелби.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tOrigamiKiller(tOrigamiKiller other) : base(other) { }
        public override object Clone() => new tOrigamiKiller(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти вражеской карты <i>{cardName}</i> от любого источника (П{PRIORITY})",
                    $"увеличивает силу владельца на {_strengthF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(32, stacks);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(e.trait.GuidStr, OnTargetPostKilled, PRIORITY);
            else e.target.OnPostKilled.Remove(e.trait.GuidStr);
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            if (target.Data.id != CARD_ID) return;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(target.Territory);
            if (trait == null) return;
            BattleFieldCard owner = trait.Owner;

            await trait.AnimActivation();
            await owner.Strength.AdjustValueScale(_strengthF.Value(trait), trait);
        }
    }
}
