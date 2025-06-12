using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDoubleAttack : PassiveTrait
    {
        const string ID = "double_attack";
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tDoubleAttack() : base(ID)
        {
            name = "Двойная атака";
            desc = "И тебе по лицу, и тебе по лицу!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tDoubleAttack(tDoubleAttack other) : base(other) { }
        public override object Clone() => new tDoubleAttack(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед каждой последующей атакой владельца</color>\n" +
                   $"Если цель атаки только одна - карты рядом с целью так же станут целями (исключая первоначальную цель). Сила атаки будет распределена равномерно.";
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.Receivers.Count != 1) return;

            await trait.AnimActivation();
            BattleField singleField = e.Receivers[0];
            IEnumerable<BattleField> fields = owner.Territory.Fields(singleField.pos, _range);

            e.ClearReceivers();
            foreach (BattleField field in fields)
                e.AddReceiver(field);

            await e.Strength.SetValue((float)e.Strength / e.Receivers.Count, trait);
        }
    }
}
