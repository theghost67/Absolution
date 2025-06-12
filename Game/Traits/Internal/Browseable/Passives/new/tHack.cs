using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHack : PassiveTrait
    {
        const string ID = "hack";

        public tHack() : base(ID)
        {
            name = "Взлом";
            desc = "Инициализация absolution_cracker.exe... Инициализация завершена! Ожидание блокировки аккаунта за эксплойты...";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tHack(tHack other) : base(other) { }
        public override object Clone() => new tHack(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После установки владельца на территорию впервые</color>\nВозвращает стоимость установки владельца.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || !owner.FirstFieldAttachment) return;

            await trait.AnimActivation();
            if (owner.Data.price.currency.id == "gold")
                 await trait.Side.Gold.AdjustValue(owner.Price, trait);
            else await trait.Side.Ether.AdjustValue(owner.Price, trait);
        }
    }
}
