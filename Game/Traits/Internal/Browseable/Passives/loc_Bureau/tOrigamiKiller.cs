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
            range = BattleRange.none;
        }
        protected tOrigamiKiller(tOrigamiKiller other) : base(other) { }
        public override object Clone() => new tOrigamiKiller(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            float effect = DAMAGE_REL_INCREASE_PER_STACK * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После убийства карты <i>{cardName}</i> (П{PRIORITY})",
                    $"увеличивает силу владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 32 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerKillConfirmed(object sender, BattleFieldCard victim)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;
            if (victim.Data.id != CARD_ID) return;

            await trait.AnimActivation();
            await owner.strength.AdjustValueScale(DAMAGE_REL_INCREASE_PER_STACK * trait.GetStacks(), trait);
        }
    }
}
