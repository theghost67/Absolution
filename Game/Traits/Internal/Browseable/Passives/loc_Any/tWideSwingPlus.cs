using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWideSwingPlus : PassiveTrait
    {
        const string ID = "wide_swing_plus";
        const int PRIORITY = 2;
        static readonly TerritoryRange _range = TerritoryRange.ownerAll;

        public tWideSwingPlus() : base(ID)
        {
            name = "Широкий замах+";
            desc = "ЩАС КАК ДАМ!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tWideSwingPlus(tWideSwingPlus other) : base(other) { }
        public override object Clone() => new tWideSwingPlus(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед каждой последующей атакой владельца (П{PRIORITY})</color>\n" +
                   $"Если цель атаки только одна - все карты на стороне цели станут целями. Сила атаки будет распределена равномерно.";
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
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
