using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCompetitiveObsession : PassiveTrait
    {
        const string ID = "competitive_obsession";
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.10f);
        const string STRENGTH_STORAGE_ID = "strength";

        public tCompetitiveObsession() : base(ID)
        {
            name = Translator.GetString("trait_competitive_obsession_1");
            desc = Translator.GetString("trait_competitive_obsession_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tCompetitiveObsession(tCompetitiveObsession other) : base(other) { }
        public override object Clone() => new tCompetitiveObsession(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            args.data.storage.TryGetValue(STRENGTH_STORAGE_ID, out object bonus);
            string text = Translator.GetString("trait_competitive_obsession_3", _strengthF.Format(args.stacks, true));

            if (bonus != null && (float)bonus != 0)
                text += Translator.GetString("trait_competitive_obsession_4", (float)bonus * 100);
            return text;
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(10, stacks, 1, 1.75f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);

            if (e.trait.WasAdded(e))
            {
                if (!e.trait.Data.storage.ContainsKey(STRENGTH_STORAGE_ID))
                    e.trait.Data.storage[STRENGTH_STORAGE_ID] = 0f;
                if (e.isInBattle && !((IBattleTrait)e.trait).Side.isMe)
                    e.trait.Data.storage[STRENGTH_STORAGE_ID] = Menu.GetCurrent() is BattlePlaceMenu menu ? Utils.RandomIntSafe(0, menu.DemoDifficulty) : 0f;

                float strengthRel = (float)e.trait.Data.storage[STRENGTH_STORAGE_ID];
                if (strengthRel > 0)
                    await e.trait.Owner.Strength.AdjustValueScale((float)strengthRel, e.trait);

                if (!e.isInBattle) return;
                ((BattleFieldCard)e.trait.Owner).Territory.OnPlayerWon.Add(e.trait.GuidStr, OnPlayerWon);
            }
            else if (e.trait.WasRemoved(e))
            {
                if (!e.isInBattle) return;
                ((BattleFieldCard)e.trait.Owner).Territory.OnPlayerWon.Remove(e.trait.GuidStr);
            }
        }

        async UniTask OnPlayerWon(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            Trait data = trait.Data;
            TraitStorage traitStorage = data.storage;
            if (traitStorage.TryGetValue(STRENGTH_STORAGE_ID, out object value))
                 traitStorage[STRENGTH_STORAGE_ID] = ((float)value) + _strengthF.valuePerStack;
            else traitStorage[STRENGTH_STORAGE_ID] = _strengthF.valuePerStack;
        }
    }
}
