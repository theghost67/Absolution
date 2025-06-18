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
            Translator.GetString("trait_exceptional_1"),
            Translator.GetString("trait_exceptional_2"),
            Translator.GetString("trait_exceptional_3"),
            Translator.GetString("trait_exceptional_4"),
            Translator.GetString("trait_exceptional_5"),

            Translator.GetString("trait_exceptional_6"),
            Translator.GetString("trait_exceptional_7"),
            Translator.GetString("trait_exceptional_8"),
            Translator.GetString("trait_exceptional_9"),
            Translator.GetString("trait_exceptional_10"),
        };

        public tExceptional() : base(ID)
        {
            name = Translator.GetString("trait_exceptional_11");
            desc = Translator.GetString("trait_exceptional_12");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tExceptional(tExceptional other) : base(other) { }
        public override object Clone() => new tExceptional(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            if (args.table == null)
                 return Translator.GetString("trait_exceptional_13");
            else if (args.table.Owner.Data.id == CARD_ID)
                 return Translator.GetString("trait_exceptional_14");
            else return Translator.GetString("trait_exceptional_15");
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
