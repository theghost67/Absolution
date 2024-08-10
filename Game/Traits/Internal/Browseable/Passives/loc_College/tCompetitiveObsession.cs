using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCompetitiveObsession : PassiveTrait
    {
        const string ID = "competitive_obsession";
        const int PRIORITY = 0;
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.01f);
        static readonly TraitStatFormula _healthF = new(true, 0, 0.3333f);
        const string STRENGTH_STORAGE_ID = "0";
        const string HEALTH_STORAGE_ID = "1";

        public tCompetitiveObsession() : base(ID)
        {
            name = "Соревновательная одержимость";
            desc = "Раз на раз в кс пойдём, выйдем.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tCompetitiveObsession(tCompetitiveObsession other) : base(other) { }
        public override object Clone() => new tCompetitiveObsession(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После победы в сражении (П{PRIORITY})",
                    $"навсегда увеличивает силу на {_strengthF.Format(trait)}."),
                new($"После поражения в сражении (П{PRIORITY})",
                    $"навсегда уменьшает здоровье на {_healthF.Format(trait)}."),
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

            if (!trait.Side.isMe)
            {
                TraitStorage traitStorage = trait.Data.storage;
                traitStorage[STRENGTH_STORAGE_ID] = UnityEngine.Random.Range(0, 20);
                return;
            }

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

            bool hasStrengthEffect = traitStorage.TryGetValue(STRENGTH_STORAGE_ID, out object strengthRel);
            bool hasHealthEffect = traitStorage.TryGetValue(HEALTH_STORAGE_ID, out object healthRel);
            if (!hasStrengthEffect && !hasHealthEffect) return;

            await trait.AnimActivation();
            await owner.Strength.AdjustValueScale((float)strengthRel, trait);
            await owner.Health.AdjustValueScale((float)healthRel, trait);
        }
        async UniTask OnPlayerWon(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null) return;

            Trait data = trait.Data;
            TraitStorage traitStorage = data.storage;
            if (traitStorage.TryGetValue(STRENGTH_STORAGE_ID, out object value))
                 traitStorage[STRENGTH_STORAGE_ID] = ((float)value) + _strengthF.valuePerStack;
            else traitStorage[STRENGTH_STORAGE_ID] = _strengthF.valuePerStack;

            await trait.AnimActivation();
        }
        async UniTask OnPlayerLost(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null) return;

            Trait data = trait.Data;
            TraitStorage traitStorage = data.storage;
            if (traitStorage.TryGetValue(HEALTH_STORAGE_ID, out object value))
                 traitStorage[HEALTH_STORAGE_ID] = ((float)value) - _healthF.valuePerStack;
            else traitStorage[HEALTH_STORAGE_ID] = -_healthF.valuePerStack;

            await trait.AnimActivation();
        }
    }
}
