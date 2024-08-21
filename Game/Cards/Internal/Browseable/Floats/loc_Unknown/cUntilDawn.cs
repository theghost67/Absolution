using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System;

namespace Game.Cards
{
    public class cUntilDawn : FloatCard
    {
        const string ID = "until_dawn";
        const string TRAIT_ID = "till_dawn";
        const int PRIORITY = 1;
        string _eventGuid;
        TableFinder _sideFinder;

        public cUntilDawn() : base(ID)
        {
            name = "Дожить до рассвета";
            desc = "Итак... ты хочешь приступить к этой \"игре\"?";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cUntilDawn(cUntilDawn other) : base(other) { }
        public override object Clone() => new cUntilDawn(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"До следующего хода, все карты на территории (П{PRIORITY}) получают навык <u>{traitName}</u>.";
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);
            if (!e.isInBattle) return;

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide side = card.Side;
            BattleTerritory terr = side.Territory;

            _eventGuid = card.GuidStr;
            _sideFinder = side.Finder;

            terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add, PRIORITY);
            terr.OnStartPhase.Add(_eventGuid, OnStartPhase);
        }

        UniTask OnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            terr.ContinuousAttachHandler_Remove(_eventGuid, ContinuousAttach_Remove);
            return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            if (e.card.FieldsAttachments == 1 || e.source == null)
                return e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
            else return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            if (e.card.FieldsAttachments == 1 || e.source == null)
                return e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
            else return UniTask.CompletedTask;
        }
    }
}
