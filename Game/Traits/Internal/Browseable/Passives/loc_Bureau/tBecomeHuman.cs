using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBecomeHuman : PassiveTrait
    {
        const string ID = "become_human";
        const int PRIORITY = 4;
        const string OBS_CARD_ID = "connor";
        static readonly TraitStatFormula _strengthF = new(true, 0.25f, 0.00f);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tBecomeHuman() : base(ID)
        {
            name = "Стать человеком";
            desc = "- Ты просто долбанная машина!\n- Разумеется, Лейтенант, я машина. А вы ожидали чего-то другого?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf, PRIORITY);
        }
        protected tBecomeHuman(tBecomeHuman other) : base(other) { }
        public override object Clone() => new tBecomeHuman(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            return $"<color>При появлении карты <nobr><u>{cardName}</u></nobr> на союзной стороне (П{PRIORITY})</color>\n" +
                   $"Увеличивает свою силу на {_strengthF.Format(args.stacks)} и инициативу на {_moxieF.Format(args.stacks, true)}.";
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
                await owner.Strength.AdjustValueScale(_strengthF.Value(e.traitStacks), trait, guid);
                await owner.Moxie.AdjustValue(_moxieF.Value(e.traitStacks), trait, guid);
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
