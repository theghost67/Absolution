using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    public class tShift : PassiveTrait
    {
        const string ID = "shift";
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tShift() : base(ID)
        {
            name = Translator.GetString("trait_shift_1");
            desc = Translator.GetString("trait_shift_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tShift(tShift other) : base(other) { }
        public override object Clone() => new tShift(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_shift_3", _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks, 1);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null || owner.IsKilled) return;

            BattleField[] fields = trait.Side.Fields().ToArray();
            if (fields.Length == 0) return;

            await trait.AnimActivation();

            int posX = owner.Field.pos.x;
            BattleField targetField = null;
            for (int i = posX + 1; i < fields.Length; i++)
            {
                if (fields[i].pos.x <= posX || fields[i].Card != null) continue;
                targetField = fields[i];
                break;
            }
            targetField ??= fields.First();

            if (owner.Drawer == null)
                await owner.TryAttachToField(targetField, trait);
            else
            {
                owner.Drawer.Alpha = 0;
                await owner.TryAttachToField(targetField, trait);
                owner.Drawer.DOFade(1, 1).SetEase(Ease.OutQuad);
            }

            await owner.Strength.AdjustValue(_strengthF.ValueInt(trait.GetStacks()), trait);
        }
    }
}
