using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tException : PassiveTrait
    {
        const string ID = "exception";

        public tException() : base(ID)
        {
            name = "Исключение";
            desc = "Вас пора исключить.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tException(tException other) : base(other) { }
        public override object Clone() => new tException(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После убийства любой карты владельцем</color>\nУбивает карты с таким же идентификатором, как у жертвы.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }
        static async UniTask OnKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard[] cards = trait.Territory.Fields().WithCard().Where(f => !f.Card.IsKilled && f.Card.Data.id == e.victim.Data.id).Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            await trait.AnimActivation();
            foreach (BattleFieldCard card in cards)
                await card.TryKill(BattleKillMode.Default, trait);
        }
    }
}
