using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMiracleAftertaste : PassiveTrait
    {
        const string ID = "miracle_aftertaste";
        const string KEY = "turn";
        static readonly TraitStatFormula _moxieBuffF = new(false, 1, 0);
        static readonly TraitStatFormula _moxieDebuffF = new(false, 1, 0);
        static readonly TraitStatFormula _strengthBuffF = new(true, 0.25f, 0.00f);
        static readonly TraitStatFormula _statsDebuffF = new(true, 0.25f, 0.25f);

        public tMiracleAftertaste() : base(ID)
        {
            name = "Чудодейственное послевкусие";
            desc = "Это что же значит? ..Да, Лёва, да, через 5 минут тебе беда Лёва, да.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tMiracleAftertaste(tMiracleAftertaste other) : base(other) { }
        public override object Clone() => new tMiracleAftertaste(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении</color>\nПовышает инициативу владельца на {_moxieBuffF.Format(args.stacks)} " +
                   $"и его атаку на {_strengthBuffF.Format(args.stacks, true)}. В конце следующего хода уменьшает инициативу владельца на {_moxieDebuffF.Format(args.stacks)} " +
                   $"и понижает его здоровье и силу на {_statsDebuffF.Format(args.stacks)}, удаляет все заряды (ранее наложенные бонусы тоже удаляются).";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                await trait.Owner.Moxie.AdjustValue(_moxieBuffF.Value(trait.GetStacks()), trait, trait.GuidStr);
                await trait.Owner.Strength.AdjustValueScale(_strengthBuffF.Value(trait.GetStacks()), trait, trait.GuidStr);
                if (trait.Owner.IsKilled) return;
                trait.Territory.OnEndPhase.Add(trait.GuidStr, OnEndPhase);
            }
            else if (trait.WasRemoved(e))
            {
                await trait.Owner.Moxie.RevertValue(trait.GuidStr);
                await trait.Owner.Strength.RevertValueScale(trait.GuidStr);
                if (trait.Owner.IsKilled) return;
                trait.Territory.OnEndPhase.Remove(trait.GuidStr);
            }
        }
        async UniTask OnEndPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            if (!trait.Storage.ContainsKey(KEY))
            {
                trait.Storage[KEY] = null;
                return;
            }

            BattleFieldCard owner = trait.Owner;
            if (trait.Owner.IsKilled) return;

            float debuff = _statsDebuffF.Value(trait.GetStacks());
            await trait.AnimActivation();
            await owner.Health.AdjustValueScale(-debuff, trait);
            await owner.Strength.AdjustValueScale(-debuff, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
