using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using System;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHardened : PassiveTrait
    {
        const string ID = "hardened";
        const string VALUE_KEY = "value";
        const string TURN_KEY = "turn";
        static readonly TraitStatFormula _valueBaseF = new(false, 0, 2);
        static readonly TraitStatFormula _valuePerTurnF = new(false, 0, 1);

        public tHardened() : base(ID)
        {
            name = Translator.GetString("trait_hardened_1");
            desc = Translator.GetString("trait_hardened_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tHardened(tHardened other) : base(other) { }
        public override object Clone() => new tHardened(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object value = null;
            bool hasKey = args.table?.Storage.TryGetValue(VALUE_KEY, out value) ?? false;
            string str = Translator.GetString("trait_hardened_3", _valueBaseF.Format(args.stacks, true), _valuePerTurnF.Format(args.stacks, true));

            if (hasKey)
                str += Translator.GetString("trait_hardened_4", value);
            return str;
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.5f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Storage[VALUE_KEY] = 0;
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived);
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
            }
        }
        static async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled || e.Strength <= 0) return;

            int value = (int)trait.Storage[VALUE_KEY];
            if (value <= 0) return;

            await trait.AnimActivation();
            await e.Strength.AdjustValue(-value, trait);
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;
            if (!trait.Storage.ContainsKey(TURN_KEY))
            {
                trait.Storage[TURN_KEY] = null;
                return;
            }
            await trait.AnimActivationShort();
            trait.Storage[VALUE_KEY] = (int)trait.Storage[VALUE_KEY] + _valuePerTurnF.ValueInt(trait.GetStacks());
            trait.Storage.Remove(TURN_KEY);
        }
    }
}
