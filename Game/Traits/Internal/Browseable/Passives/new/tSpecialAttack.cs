using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSpecialAttack : PassiveTrait
    {
        const string ID = "special_attack";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tSpecialAttack() : base(ID)
        {
            name = "Особая атака";
            desc = "Да, это моя особая атака. Я буду просто стоять здесь.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tSpecialAttack(tSpecialAttack other) : base(other) { }
        public override object Clone() => new tSpecialAttack(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Постоянный эффект во время боя</color>\nУвеличивает инициативу владельца на {_moxieF.Format(args.stacks)}\n\n" +
                   $"<color>Перед совершением атаки владельцем</color>\nЕсли на вражеской территории нет противников, пропускает ход.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
                await trait.Owner.Moxie.AdjustValue(_moxieF.ValueInt(trait.GetStacks()), trait, trait.GuidStr);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
                if (!trait.Owner.IsKilled)
                    await trait.Owner.Moxie.RevertValue(trait.GuidStr);
            }
        }
        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled || e.Strength <= 0) return;

            BattleField[] fieldsWithCard = trait.Side.Opposite.Fields().WithCard().ToArray();
            if (fieldsWithCard.Length != 0) return;

            await trait.AnimActivation();
            e.handled = true;
        }
    }
}
