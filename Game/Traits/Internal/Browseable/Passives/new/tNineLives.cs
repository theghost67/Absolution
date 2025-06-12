using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tNineLives : PassiveTrait
    {
        const string ID = "nine_lives";
        static readonly TraitStatFormula _initialLivesF = new(false, 9, 0);
        static readonly TraitStatFormula _statsDebuffF = new(true, 1.00f, 0);

        public tNineLives() : base(ID)
        {
            name = "Девять жизней";
            desc = "Всё в порядке, у меня ведь девять жизней! ...Да? И сколько жизней у тебя осталось?";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tNineLives(tNineLives other) : base(other) { }
        public override object Clone() => new tNineLives(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string livesStr = _initialLivesF.Format(args.stacks);
            string statsDebuffStr = _statsDebuffF.Format(args.stacks);
            return $"<color>После смерти владельца</color>\nНавсегда даёт 1 ед. заряда навыка. Если зарядов было меньше {livesStr}, " +
                   $"создаёт копию владельца с единственным навыком <u>{name}</u> и уменьшенными на {statsDebuffStr} характеристиками, " +
                   $"помещая её в рукав стороны-владельца. Если на момент смерти зарядов было {livesStr} и более, навсегда удаляет карту из колоды, тратит все заряды.";
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
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            int stacks = trait.GetStacks();
            if (stacks >= _initialLivesF.ValueInt(stacks))
            {
                if (trait.Side.isMe)
                    Player.Deck.fieldCards.Remove(owner.Data);
                await trait.SetStacks(0, trait);
                return;
            }

            owner.Data.traits.Passives.AdjustStacks(ID, 1);

            FieldCard card = (FieldCard)owner.Data.CloneAsNew();
            float statsDebuff = _statsDebuffF.Value(stacks);
            card.health = (int)Math.Ceiling(card.strength * (1 / (1 + statsDebuff)));
            card.strength = (int)Math.Ceiling(card.strength * (1 / (1 + statsDebuff)));
            card.traits.Clear();
            card.traits.AdjustStacks(trait.Data, stacks + 1);
            owner.Side.Sleeve.Add(card);
        }
    }
}
