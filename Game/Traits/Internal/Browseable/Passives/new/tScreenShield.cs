using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;
using Game.Effects;
using System;
using System.Linq;
using Game.Palette;
using GreenOne;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScreenShield : PassiveTrait
    {
        const string ID = "screen_shield";
        const string SHIELD_HEALTH_MAX_KEY = "shield_health_max";
        const string SHIELD_HEALTH_CUR_KEY = "shield_health_cur";
        const string SHIELD_RESTORE_KEY = "shield_restore";

        static readonly TraitStatFormula _healthF = new(false, 4, 2);
        static readonly TraitStatFormula _restoreF = new(true, 0.20f, 0.00f);

        public tScreenShield() : base(ID)
        {
            name = Translator.GetString("trait_screen_shield_1");
            desc = Translator.GetString("trait_screen_shield_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerTriple);
        }
        protected tScreenShield(tScreenShield other) : base(other) { }
        public override object Clone() => new tScreenShield(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string str = Translator.GetString("trait_screen_shield_3", _healthF.Format(args.stacks), _restoreF.Format(args.stacks));

            if (args.table != null && args.table.Storage.ContainsKey(SHIELD_HEALTH_CUR_KEY))
                str += Translator.GetString("trait_screen_shield_4", args.table.Storage[SHIELD_HEALTH_CUR_KEY], args.table.Storage[SHIELD_RESTORE_KEY]);
            return str;
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(10, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached);
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
                trait.Storage.Remove(SHIELD_HEALTH_CUR_KEY);
                trait.Storage.Remove(SHIELD_RESTORE_KEY);
                await trait.Owner.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
            }
        }

        async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || !owner.FirstFieldAttachment)
            {
                await trait.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
                return;
            }

            int stacks = trait.GetStacks();
            int health = _healthF.ValueInt(stacks);
            float restoreShare = _restoreF.Value(stacks);
            int healthRestore = (int)Mathf.Ceil(health * restoreShare);
            await trait.AnimActivation();
            trait.Storage[SHIELD_HEALTH_MAX_KEY] = health;
            trait.Storage[SHIELD_HEALTH_CUR_KEY] = health;
            trait.Storage[SHIELD_RESTORE_KEY] = healthRestore;
            await trait.Territory.ContinuousAttachHandler_Add(trait.GuidStr, ContinuousAttach_Add, trait.Owner);
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null || owner.IsKilled || !trait.Storage.ContainsKey(SHIELD_HEALTH_CUR_KEY)) return;

            int restoreHealth = (int)trait.Storage[SHIELD_RESTORE_KEY];
            int maxHealth = (int)trait.Storage[SHIELD_HEALTH_MAX_KEY];
            int oldHealth = (int)trait.Storage[SHIELD_HEALTH_CUR_KEY];
            int newHealth = (oldHealth + restoreHealth).ClampedMax(maxHealth);
            if (oldHealth == newHealth) return;

            restoreHealth = newHealth - oldHealth;
            trait.Storage[SHIELD_HEALTH_CUR_KEY] = newHealth;
            await trait.AnimActivationShort($"{trait.Data.name}\n+{restoreHealth}");
        }

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null)
                card.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait != null)
                card.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(card.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled || e.Strength <= 0) return;

            bool isInRange = trait.Territory.Fields(trait.Field.pos, trait.Data.range.potential).Contains(card.Field);
            if (!isInRange) return;

            int initialStrength = e.Strength;
            int shieldHealth = (int)trait.Storage[SHIELD_HEALTH_CUR_KEY];
            if (shieldHealth - initialStrength >= 0)
                 await e.Strength.SetValue(0, trait);
            else await e.Strength.AdjustValue(-shieldHealth, trait);
            shieldHealth -= initialStrength;
            if (shieldHealth <= 0)
            {
                trait.Owner.Drawer?.CreateTextAsSpeech(Translator.GetString("trait_screen_shield_5", trait.Data.name), Color.red);
                await trait.SetStacks(0, trait);
            }
            else
            {
                trait.Storage[SHIELD_HEALTH_CUR_KEY] = shieldHealth;
                trait.Owner.Drawer?.CreateTextAsSpeech($"{trait.Data.name}\n-{initialStrength}", Color.red);
            }
        }
    }
}
