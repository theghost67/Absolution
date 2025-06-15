using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using MyBox;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExceptional : PassiveTrait
    {
        const string ID = "exceptional";
        const string CARD_ID = "incredible";
        static readonly string[] _names = new string[]
        {
            "Мистер Исключительный",
            "Мистер Исключающий",
            "Мистер Искупительный",
            "Мистер Изумительный",
            "Мистер Извратительный",

            "Мистер Заключительный",
            "Мистер Выключительный",
            "Мистер Иссушительный",
            "Мистер Удушительный",
            "Мистер Пояснительный",
        };

        public tExceptional() : base(ID)
        {
            name = "Мистер Исключительный";
            desc = "Я Мистер Исключительный! Не Мистер Ни-то ни-сё, Мистер... Исключительный!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tExceptional(tExceptional other) : base(other) { }
        public override object Clone() => new tExceptional(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            if (args.table == null)
                 return "Имеет достаточно много имён.";
            else if (args.table.Owner.Data.id == CARD_ID)
                 return $"Имеет достаточно много имён. Так ведь, Мистер...?";
            else return $"Эй, это не Мистер Исключительный! И как же тебя называть?";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            ITableTrait trait = e.trait;
            if (!trait.WasAdded(e)) return;
            if (trait.Owner.Data.id != CARD_ID) return;
            trait.Owner.OnDrawerCreated += (s, e) => RedrawOwnerName(trait.Owner);
            RedrawOwnerName(trait.Owner);
        }

        private void RedrawOwnerName(TableFieldCard owner)
        {
            string name = _names.RandomSafe();
            owner.Data.name = name;
            owner.Drawer?.RedrawHeader(name);
        }
    }
}
