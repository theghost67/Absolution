using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tUltrasonicScream : PassiveTrait
    {
        const string ID = "ultrasonic_scream";
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);
        static readonly TerritoryRange _range = TerritoryRange.oppositeTriple;

        public tUltrasonicScream() : base(ID)
        {
            name = Translator.GetString("trait_ultrasonic_scream_1");
            desc = Translator.GetString("trait_ultrasonic_scream_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.allNotSelf);
        }
        protected tUltrasonicScream(tUltrasonicScream other) : base(other) { }
        public override object Clone() => new tUltrasonicScream(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_ultrasonic_scream_3", _moxieF.Format(args.stacks, true));

        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(e.trait.GuidStr, OnTargetPostKilled);
            else e.target.OnPostKilled.Remove(e.trait.GuidStr);
        }
        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(target.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Owner.Territory.Fields(trait.Owner.Field.pos, _range).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int value = -_moxieF.ValueInt(trait.GetStacks());
            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.Moxie.AdjustValue(value, trait);
        }
    }
}
