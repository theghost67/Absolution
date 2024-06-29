using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tCompetitiveObsession : PassiveTrait
    {
        const string ID = "competitive_obsession";
        const int PRIORITY = 0;
        const float STRENGTH_REL_INCREASE_WHEN_WON = 0.01f;
        const float HEALTH_REL_DECREASE_WHEN_LOST = 0.50f;
        const int STRENGTH_REL_STORAGE_INDEX = 0;
        const int HEALTH_REL_STORAGE_INDEX = 0;
        int _ownerDeckGuid;

        public tCompetitiveObsession() : base(ID)
        {
            name = "Соревновательная одержимость";
            desc = "Раз на раз в кс пойдём, выйдем.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tCompetitiveObsession(tCompetitiveObsession other) : base(other) { _ownerDeckGuid = other._ownerDeckGuid; }
        public override object Clone() => new tCompetitiveObsession(this);

        public override string DescRich(ITableTrait trait)
        {
            float strengthEffect = STRENGTH_REL_INCREASE_WHEN_WON * 100 * trait.GetStacks();
            float healthEffect = HEALTH_REL_DECREASE_WHEN_LOST * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После победы в сражении (П{PRIORITY})",
                    $"навсегда увеличивает силу на <u>{strengthEffect}%</u>."),
                new($"После поражения в сражении (П{PRIORITY})",
                    $"навсегда уменьшает здоровье на <u>{healthEffect}%</u>."),
                new($"После установки владельца на поле (П{PRIORITY})",
                    $"применяет вышеуказанные бонусы к силе и здоровью."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 8 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;
            if (!trait.Side.isMe) return;
            _ownerDeckGuid = trait.Owner.Data.Guid;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnFieldPostAttached.Add(OnOwnerFieldPostAttached, PRIORITY);
                trait.Owner.Territory.OnPlayerWon.Add(OnPlayerWon, PRIORITY);
                trait.Owner.Territory.OnPlayerLost.Add(OnPlayerLost, PRIORITY);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnFieldPostAttached.Remove(OnOwnerFieldPostAttached);
                trait.Owner.Territory.OnPlayerWon.Remove(OnPlayerWon);
                trait.Owner.Territory.OnPlayerLost.Remove(OnPlayerLost);
            }
        }

        async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            PassiveTrait traitData = trait.Data;
            TraitStorage traitStorage = traitData.storage;

            bool hasStrengthEffect = traitStorage.TryGetValue(STRENGTH_REL_STORAGE_INDEX, out object strengthRel);
            bool hasHealthEffect = traitStorage.TryGetValue(HEALTH_REL_STORAGE_INDEX, out object healthRel);
            if (!hasStrengthEffect && !hasHealthEffect) return;

            await trait.AnimActivation();
            await owner.strength.AdjustValueScale((float)strengthRel, trait);
            await owner.health.AdjustValueScale((float)healthRel, trait);
        }
        async UniTask OnPlayerWon(object sender, EventArgs e)
        {
            FieldCard ownerData = Player.Deck.fieldCards[_ownerDeckGuid];
            if (ownerData == null) return;

            PassiveTrait traitData = ownerData.traits.Passive(ID);
            if (traitData == null) return;

            TraitStorage traitStorage = traitData.storage;
            if (traitStorage.TryGetValue(STRENGTH_REL_STORAGE_INDEX, out object value))
                 traitStorage[STRENGTH_REL_STORAGE_INDEX] = ((float)value) + STRENGTH_REL_INCREASE_WHEN_WON;
            else traitStorage[STRENGTH_REL_STORAGE_INDEX] = STRENGTH_REL_INCREASE_WHEN_WON;
        }
        async UniTask OnPlayerLost(object sender, EventArgs e)
        {
            FieldCard ownerData = Player.Deck.fieldCards[_ownerDeckGuid];
            if (ownerData == null) return;

            PassiveTrait traitData = ownerData.traits.Passive(ID);
            if (traitData == null) return;

            TraitStorage traitStorage = traitData.storage;
            if (traitStorage.TryGetValue(HEALTH_REL_STORAGE_INDEX, out object value))
                 traitStorage[HEALTH_REL_STORAGE_INDEX] = ((float)value) - HEALTH_REL_DECREASE_WHEN_LOST;
            else traitStorage[HEALTH_REL_STORAGE_INDEX] = -HEALTH_REL_DECREASE_WHEN_LOST;
        }
    }
}
