using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLightningSpeed : ActiveTrait
    {
        const string ID = "lightning_speed";
        const int CD = 2;

        public tLightningSpeed() : base(ID)
        {
            name = "Молниеностность";
            desc = "Быстрее быстрого, скорее скорого. Скорость. Я - скорость.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tLightningSpeed(tLightningSpeed other) : base(other) { }
        public override object Clone() => new tLightningSpeed(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на незанятом союзном поле</color>\nПеремещает владельца на выбранное поле. Перезарядка: {CD} х.";
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            await owner.TryAttachToField(target, trait);
            trait.SetCooldown(CD);
        }
    }
}
