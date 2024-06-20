using Game.Cards;
using Game;
using Game.Environment;
using Game.Territories;
using Game.Territories;
using Unity.Mathematics;
using UnityEngine;
using Game.Sleeves;

namespace Game.Menus
{
    /// <summary>
    /// Абстрактный класс, представляющий меню для взаимодействия с используемым (при заполнении территории картами) местом локации (см. <see cref="LocationPlace"/>).
    /// </summary>
    public abstract class PlaceMenuUsable : PlaceMenu
    {
        static PlaceMenuUsable _instance;

        protected override string LeftText => "Заполните все игровые поля картами,\nчтобы активировать данное место.";
        protected bool IsUsed => _isUsed;

        protected readonly TableTerritory territory;
        protected readonly TableSleeve sleeve;

        bool _isUsed;

        sealed class pSleeve : TableSleeve
        {
            public pSleeve(Transform transform) : base(Player.Deck, true, transform) { }
            protected override ITableSleeveCard HoldingCardCreator(Card data) => _instance.SleeveCardCreator(data);
        }
        sealed class pTerritory : TableTerritory
        {
            readonly int _totalFields;
            int _attachedFields;

            public pTerritory(int2 grid) : base(grid, parent: _instance.Transform)
            {
                _totalFields = grid.x * grid.y;
                throw new System.NotImplementedException();

                //OnCardAttachedToField.Add(OnAttach);
                //OnCardDetatchedFromField.Add(OnDetatch);
            }

            void OnAttach(object sender, TableField field)
            {
                pTerritory terr = (pTerritory)sender;
                terr._attachedFields++;

                if (terr._attachedFields < terr._totalFields) return;
                if (!_instance.TryUse()) return;

                _instance._isUsed = true;
                _instance.SetColliders(false);
            }
            void OnDetatch(object sender, TableField field)
            {
                pTerritory terr = (pTerritory)sender;
                terr._attachedFields--;
            }
        }

        protected PlaceMenuUsable(string name, int2 territoryGrid) : base(name, UIFlags.WithAll ^ UIFlags.WithDeckButton)
        {
            _instance = this;
            territory = new pTerritory(territoryGrid);
            sleeve = new pSleeve(Transform.Find("Sleeve"));
            sleeve.TakeMissingCards();
        }
        public override void SetColliders(bool value)
        {
            base.SetColliders(value);
            sleeve.Drawer.SetCollider(value);
            territory.SetCardsColliders(value);
        }

        protected abstract bool TryUse();
        protected abstract ITableSleeveCard SleeveCardCreator(Card data);
    }
}
