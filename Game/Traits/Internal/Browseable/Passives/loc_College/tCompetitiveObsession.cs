﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCompetitiveObsession : PassiveTrait
    {
        const string ID = "competitive_obsession";
        const int PRIORITY = 0;
        const float STRENGTH_REL_INCREASE_WHEN_WON = 0.01f;
        const float HEALTH_REL_DECREASE_WHEN_LOST = 0.50f;
        const string STRENGTH_REL_STORAGE_ID = ID + ":0";
        const string HEALTH_REL_STORAGE_ID = ID + ":1";
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

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;

            if (!trait.Side.isMe) return;
            _ownerDeckGuid = trait.Owner.Data.Guid;

            if (trait.WasAdded(e))
            {
                owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached, PRIORITY);
                owner.Territory.OnPlayerWon.Add(trait.GuidStr, OnPlayerWon, PRIORITY);
                owner.Territory.OnPlayerLost.Add(trait.GuidStr, OnPlayerLost, PRIORITY);
            }
            else if (trait.WasRemoved(e))
            {
                owner.OnFieldPostAttached.Remove(trait.GuidStr);
                owner.Territory.OnPlayerWon.Remove(trait.GuidStr);
                owner.Territory.OnPlayerLost.Remove(trait.GuidStr);
            }
        }

        async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            Trait traitData = trait.Data;
            TraitStorage traitStorage = traitData.storage;

            bool hasStrengthEffect = traitStorage.TryGetValue(STRENGTH_REL_STORAGE_ID, out object strengthRel);
            bool hasHealthEffect = traitStorage.TryGetValue(HEALTH_REL_STORAGE_ID, out object healthRel);
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
            if (traitStorage.TryGetValue(STRENGTH_REL_STORAGE_ID, out object value))
                 traitStorage[STRENGTH_REL_STORAGE_ID] = ((float)value) + STRENGTH_REL_INCREASE_WHEN_WON;
            else traitStorage[STRENGTH_REL_STORAGE_ID] = STRENGTH_REL_INCREASE_WHEN_WON;
        }
        async UniTask OnPlayerLost(object sender, EventArgs e)
        {
            FieldCard ownerData = Player.Deck.fieldCards[_ownerDeckGuid];
            if (ownerData == null) return;

            PassiveTrait traitData = ownerData.traits.Passive(ID);
            if (traitData == null) return;

            TraitStorage traitStorage = traitData.storage;
            if (traitStorage.TryGetValue(HEALTH_REL_STORAGE_ID, out object value))
                 traitStorage[HEALTH_REL_STORAGE_ID] = ((float)value) - HEALTH_REL_DECREASE_WHEN_LOST;
            else traitStorage[HEALTH_REL_STORAGE_ID] = -HEALTH_REL_DECREASE_WHEN_LOST;
        }
    }
}
