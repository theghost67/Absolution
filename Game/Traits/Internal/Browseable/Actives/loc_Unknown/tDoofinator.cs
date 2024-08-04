using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDoofinator : ActiveTrait
    {
        const string ID = "doofinator";
        const string CARD_ID = "doof";
        const int MOXIE_THRESHOLD = 0;

        public tDoofinator() : base(ID)
        {
            name = "Фуфел-инатор";
            desc = "Трепещи, Перри-утконос, Фуфел-инатор!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tDoofinator(tDoofinator other) : base(other) { }
        public override object Clone() => new tDoofinator(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Если инициатива цели ≤ {MOXIE_THRESHOLD}, она превратится в карту {cardName} с такими же характеристиками, как у владельца. Тратит один заряд."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.12f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField targetField = (BattleField)e.target;
            BattleFieldCard target = targetField.Card;
            if (target.moxie <= MOXIE_THRESHOLD)
            {
                await target.TryKill(BattleKillMode.Default, trait);
                if (target.IsKilled)
                {
                    FieldCard ownerClone = (FieldCard)trait.Owner.Data.CloneAsNew();
                    ownerClone.traits.Clear();
                    await trait.Territory.PlaceFieldCard(ownerClone, targetField, trait);
                }
            }
            await trait.AdjustStacks(-1, trait.Side);
        }
    }
}
