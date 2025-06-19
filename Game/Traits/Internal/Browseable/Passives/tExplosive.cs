using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExplosive : PassiveTrait
    {
        const string ID = "explosive";
        static readonly TraitStatFormula _strengthF = new(false, 0, 1);

        public tExplosive() : base(ID)
        {
            name = Translator.GetString("trait_explosive_1");
            desc = Translator.GetString("trait_explosive_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tExplosive(tExplosive other) : base(other) { }
        public override object Clone() => new tExplosive(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_explosive_3", _strengthF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(5, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.Field == null) return;
            if (owner.Field == null) return;

            int strength = _strengthF.ValueInt(trait.GetStacks());
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, range.potential).WithCard();

            await trait.AnimActivation();
            foreach (BattleField field in fields)
            {
                field.Card.Drawer?.CreateTextAsSpeech(Translator.GetString("trait_explosive_4", strength), Color.red);
                await field.Card.Health.AdjustValue(-strength, trait);
            }
        }
    }
}
