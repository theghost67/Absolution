using Cysharp.Threading.Tasks;
using Game.Territories;
using System;
using System.Linq;

namespace Game.Cards
{
    public class cUntilDawn : FloatCard
    {
        const string ID = "until_dawn";
        const string TRAIT_ID = "till_dawn";
        static readonly TerritoryRange _range = TerritoryRange.all;
        string _eventGuid;
        TableFinder _sideFinder;

        public cUntilDawn() : base(ID)
        {
            name = "Дожить до рассвета";
            desc = "Итак... ты хочешь приступить к этой \"игре\"?";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
            frequency = 1.00f;
        }
        protected cUntilDawn(cUntilDawn other) : base(other) { }
        public override object Clone() => new cUntilDawn(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, $"В начале следующего хода, все карты на территории, которые пережили прошлый ход, " +
                                      $"восстанавливают 33% здоровья и получают 2 ед. инициативы.");
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

            terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add);
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
            return e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            return e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
        }
    }
}
