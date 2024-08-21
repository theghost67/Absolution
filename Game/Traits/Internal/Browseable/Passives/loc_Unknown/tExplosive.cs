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
        const int PRIORITY = 4;
        static readonly TraitStatFormula _strengthF = new(false, 0, 1);
        static readonly TerritoryRange _range = TerritoryRange.oppositeTriple;

        public tExplosive() : base(ID)
        {
            name = "Взрывоопасно";
            desc = "Я, кажется, на мину наступил.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tExplosive(tExplosive other) : base(other) { }
        public override object Clone() => new tExplosive(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После смерти владельца (П{PRIORITY})</color>\n" +
                   $"Наносит рядомстоящим вражеским картам {_strengthF.Format(args.stacks)} урона.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(6, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPreKilled.Add(trait.GuidStr, OnOwnerPreKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPreKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPreKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (owner.Field == null) return;

            int strength = _strengthF.ValueInt(trait.GetStacks());
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, _range).WithCard();

            await trait.AnimActivation();
            await trait.SetStacks(0, trait);

            foreach (BattleField field in fields)
            {
                field.Card.Drawer?.CreateTextAsSpeech($"Бах\n<size=50%>-{strength}", Color.red);
                await field.Card.Health.AdjustValue(-strength, trait);
            }
        }
    }
}
