using Game.Sleeves;
using Game.Territories;

namespace Game.Menus
{
    public class MenuFinder : TableFinder
    {
        readonly string _id;
        public MenuFinder(Menu menu) : base(menu) { _id = menu.Id; }

        public override object FindInTerritory(TableTerritory territory)
        {
            return Menu.GetAny(_id);
        }
        public override object FindInSleeve(TableSleeve sleeve)
        {
            return Menu.GetAny(_id);
        }
    }
}
