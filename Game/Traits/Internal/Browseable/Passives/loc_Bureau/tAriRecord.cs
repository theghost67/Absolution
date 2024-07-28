using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAriRecord : PassiveTrait
    {
        const string ID = "ari_record";
        const int PRIORITY = 5;
        const string TRAIT_ID = "evasion";
        const int TRAITS_PER_STACK = 1;

        public tAriRecord() : base(ID)
        {
            name = "УРС запись";
            desc = "Это не первая жертва убийцы. Значит, будут ещё.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAriRecord(tAriRecord other) : base(other) { }
        public override object Clone() => new tAriRecord(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            int traitStacks = TRAITS_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти союзной карты рядом с владельцем (П{PRIORITY})",
                    $"Даёт владельцу навык <i>{traitName}</i> с <u>{traitStacks}</u> зарядами."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 120 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = (IBattleTrait)e.trait;

            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(trait.GuidStr, OnTargetPostKilled, PRIORITY);
            else e.target.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(target.Territory);

            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = TRAITS_PER_STACK * trait.GetStacks();
            await trait.AnimActivation();
            await trait.Owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
