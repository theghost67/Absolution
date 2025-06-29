using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tEgo : ActiveTrait
    {
        const string ID = "ego";
        const int CD = 2;
        static readonly TraitStatFormula _healthF = new(false, 0, 2);
        static readonly TraitStatFormula _etherF = new(false, 1, 0);

        public tEgo() : base(ID)
        {
            name = Translator.GetString("trait_ego_1");
            desc = Translator.GetString("trait_ego_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tEgo(tEgo other) : base(other) { }
        public override object Clone() => new tEgo(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_ego_3", _healthF.Format(args.stacks), _etherF.Format(args.stacks), CD);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(12, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Health <= _healthF.ValueInt(e.traitStacks);
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            FieldCard copy = (FieldCard)target.Card.Data.CloneAsNew();

            trait.SetCooldown(CD);
            await target.Card.TryKill(BattleKillMode.IgnoreEverything, trait);
            if (!owner.Side.Sleeve.Add(copy))
                owner.Drawer?.CreateTextAsSpeech(Translator.GetString("trait_ego_4"), Color.red);
        }
    }
}
