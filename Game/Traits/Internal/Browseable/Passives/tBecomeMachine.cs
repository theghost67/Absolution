using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBecomeMachine : PassiveTrait
    {
        const string ID = "become_machine";
        const string OBS_CARD_ID = "connor";
        static readonly TraitStatFormula _strengthF = new(true, 0.50f, 0.00f);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tBecomeMachine() : base(ID)
        {
            name = Translator.GetString("trait_become_machine_1");
            desc = Translator.GetString("trait_become_machine_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tBecomeMachine(tBecomeMachine other) : base(other) { }
        public override object Clone() => new tBecomeMachine(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            return Translator.GetString("trait_become_machine_3", cardName, _strengthF.Format(args.stacks), _moxieF.Format(args.stacks, true));

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new CardDescriptiveArgs(OBS_CARD_ID) };
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard owner = trait.Owner;
            string guid = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != OBS_CARD_ID) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await owner.Strength.AdjustValueScale(-_strengthF.Value(e.traitStacks), trait, guid);
                await owner.Moxie.AdjustValue(-_moxieF.Value(e.traitStacks), trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await owner.Strength.RevertValueScale(guid);
                await owner.Moxie.RevertValue(guid);
            }
        }
    }
}
