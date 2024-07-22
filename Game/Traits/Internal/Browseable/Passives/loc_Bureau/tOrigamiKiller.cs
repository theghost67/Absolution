using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

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
        const float DAMAGE_REL_INCREASE_PER_STACK = 0.33f;

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
            float effect = DAMAGE_REL_INCREASE_PER_STACK * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти вражеской карты <i>{cardName}</i> от любого источника (П{PRIORITY})",
                    $"увеличивает силу владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 32 * Mathf.Pow(stacks - 1, 2);
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
            await owner.strength.AdjustValueScale(DAMAGE_REL_INCREASE_PER_STACK * trait.GetStacks(), trait);
        }
    }
}
