﻿using Game.Cards;
using Game.Sleeves;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, предоставляющий возможность нахождения объекта типа <see cref="TableActiveTrait"/><br/>
    /// путём кэширования необходимых для поиска данных через конструктор.
    /// </summary>
    public class TableActiveTraitFinder : TableFinder
    {
        readonly string _id;
        readonly int _ownerGuid;

        public TableActiveTraitFinder(TableActiveTrait trait) : base(trait)
        {
            _id = trait.Data.id;
            _ownerGuid = trait.Owner?.Guid ?? -1;
        }

        public override object FindInTerritory(TableTerritory territory)
        {
            if (_ownerGuid == -1)
                return null;

            TableFieldCard owner = null;
            foreach (TableField field in territory.Fields().WithCard())
            {
                if (field.Card.Guid == _ownerGuid)
                {
                    owner = field.Card;
                    break;
                }
            }

            if (owner == null && territory is BattleTerritory bTerr)
                owner = bTerr.GetFromStash(_ownerGuid);

            if (owner != null)
                return owner.Traits.Actives[_id]?.Trait ?? null;
            else return null;
        }
        public override object FindInSleeve(TableSleeve sleeve)
        {
            if (_ownerGuid == -1)
                return null;

            TableFieldCard owner = null;
            foreach (ITableCard card in sleeve)
            {
                if (card.Guid == _ownerGuid)
                {
                    owner = (TableFieldCard)card;
                    break;
                }
            }

            if (owner != null)
                return owner.Traits.Actives[_id]?.Trait ?? null;
            else return null;
        }
    }
}