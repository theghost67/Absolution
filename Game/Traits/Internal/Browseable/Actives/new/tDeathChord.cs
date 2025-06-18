using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDeathChord : ActiveTrait
    {
        const string ID = "death_chord";
        const string KEY = "chords";
        static readonly TraitStatFormula _chordsF = new(false, 0, 1);

        public tDeathChord() : base(ID)
        {
            name = Translator.GetString("trait_death_chord_1");
            desc = Translator.GetString("trait_death_chord_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tDeathChord(tDeathChord other) : base(other) { }
        public override object Clone() => new tDeathChord(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object chords = null;
            args.table?.Storage.TryGetValue(KEY, out chords);
            string str = Translator.GetString("trait_death_chord_3", _chordsF.Format(args.stacks));

            if (chords != null && (int)chords > 0)
                str += Translator.GetString("trait_death_chord_4", chords);
            return str;
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            float perTarget = _chordsF.Value(result.Entity.GetStacks());
            return new(result.Entity, perTarget * 3);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                e.trait.Storage[KEY] = 0;
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            }
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && ((int)e.trait.Storage[KEY] > 0);
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleFieldCard[] targets = owner.Territory.Fields(owner.Field.pos, TerritoryRange.oppositeAll).WithCard().Select(f => f.Card).ToArray();
            int damage = _chordsF.ValueInt(e.traitStacks);
            foreach (BattleFieldCard target in targets)
            {
                target.Drawer?.CreateTextAsDamage(damage, false);
                await target.Health.AdjustValue(-damage, trait);
            }
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard owner = trait?.Owner;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || owner.IsKilled) return;

            int chords = _chordsF.ValueInt(trait.GetStacks());
            await trait.AnimActivationShort();
            trait.Storage.TryGetValue(KEY, out object chordsExisting);
            trait.Storage[KEY] = (int)(chordsExisting ?? 0) + chords;
        }
    }
}
