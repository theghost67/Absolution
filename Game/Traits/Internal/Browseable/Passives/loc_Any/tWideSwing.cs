using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWideSwing : PassiveTrait
    {
        const string ID = "wide_swing";
        const int PRIORITY = 2;
        static readonly TerritoryRange _range = TerritoryRange.ownerTriple;

        public tWideSwing() : base(ID)
        {
            name = "Широкий замах";
            desc = "Я тебе щас как дам!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tWideSwing(tWideSwing other) : base(other) { }
        public override object Clone() => new tWideSwing(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед каждой последующей атакой владельца (П{PRIORITY})",
                    $"Если цель атаки только одна - карты рядом с целью станут целями. Сила атаки будет распределена равномерно."),
            });
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

            await e.strength.SetValue((float)e.strength / e.Receivers.Count, trait);
        }
    }
}
