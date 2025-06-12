using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOpressing : PassiveTrait
    {
        const string ID = "opressing";
        static readonly TraitStatFormula _moxieBuffF = new(false, 1, 0);
        static readonly TraitStatFormula _moxieDebuffF = new(false, 1, 0);
        static readonly TraitStatFormula _strengthBuffF = new(true, 0.20f, 0.05f);

        public tOpressing() : base(ID)
        {
            name = "Угнетатель";
            desc = "Да как же ты меня З#%&?@!...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tOpressing(tOpressing other) : base(other) { }
        public override object Clone() => new tOpressing(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После убийства карты владельцем</color>\nПовышает инициативу владельца на {_moxieBuffF.Format(args.stacks)} и его силу на " +
                   $"{_strengthBuffF.Format(args.stacks)}, но понижает инициативу всех остальных союзных карт на {_moxieDebuffF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(16, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();

            int stacks = trait.GetStacks();
            float strength = _strengthBuffF.Value(stacks);
            int moxieBuff = _moxieBuffF.ValueInt(stacks);
            int moxieDebuff = _moxieBuffF.ValueInt(stacks);

            await owner.Moxie.AdjustValue(moxieBuff, trait);
            await owner.Strength.AdjustValueScale(strength, trait);

            IEnumerable<BattleFieldCard> cards = trait.Side.Fields().WithCard().Select(f => f.Card);
            foreach (BattleFieldCard card in cards)
                await card.Moxie.AdjustValue(moxieDebuff, trait);
        }
    }
}
