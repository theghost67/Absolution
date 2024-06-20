using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при использовании карты без характерстик на территории стола.
    /// </summary>
    public class TableFloatCardUseArgs
    {
        public readonly bool isInBattle;
        public readonly TableFloatCard card;
        public readonly TableTerritory territory;

        public TableFloatCardUseArgs(TableFloatCard card, TableTerritory territory)
        {
            this.card = card;
            this.territory = territory;
            isInBattle = card is IBattleCard;
        }
    }
}
