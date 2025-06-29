using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using MyBox;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tNagaKwista : PassiveTrait
    {
        const string ID = "naga_kwista";
        const string BONUS_TRAIT_ID = "scope_plus";

        public tNagaKwista() : base(ID)
        {
            name = Translator.GetString("trait_naga_kwista_1");
            desc = Translator.GetString("trait_naga_kwista_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tNagaKwista(tNagaKwista other) : base(other) { }
        public override object Clone() => new tNagaKwista(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(BONUS_TRAIT_ID).name;
            return Translator.GetString("trait_naga_kwista_3", traitName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(BONUS_TRAIT_ID) { linkFormat = true } };
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnInitiationPreSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }
        static async UniTask OnInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.Receivers.Count != 1) return;

            TerritoryRange range = owner.Traits.Active(BONUS_TRAIT_ID) != null ? TerritoryRange.oppositeAll : TerritoryRange.oppositeTriple;
            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, range).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            BattleFieldCard minHealthCard = cards.MinBy(c => c.Health);
            if (e.Receivers[0] == minHealthCard.Field) return;

            await trait.AnimActivation();
            e.ClearReceivers();
            e.AddReceiver(minHealthCard.Field);
        }
    }
}
