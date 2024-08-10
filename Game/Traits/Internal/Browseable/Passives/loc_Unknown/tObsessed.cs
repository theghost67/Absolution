using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tObsessed : PassiveTrait
    {
        const string ID = "obsessed";
        const int PRIORITY = 7;

        public tObsessed() : base(ID)
        {
            name = "Одержимость";
            desc = "Я хочу только одного: уничтожить тебя.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tObsessed(tObsessed other) : base(other) { }
        public override object Clone() => new tObsessed(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед атакой владельца в конце хода (П{PRIORITY})",
                    $"Меняет цель атаки на вражескую карту с наибольшим количеством здоровья. Тратит все заряды."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            if (owner.IsKilled || owner.Field == null) return;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            IEnumerable<BattleField> oppositeFields = owner.Territory.Fields(owner.Field.pos, TerritoryRange.oppositeAll).WithCard();
            BattleField fieldWithMaxHp = oppositeFields.FirstOrDefault();
            if (fieldWithMaxHp == null) return;
            foreach (BattleField field in oppositeFields)
            {
                if (field.Card.Health > fieldWithMaxHp.Card.Health)
                    fieldWithMaxHp = field;
            }

            await trait.AnimActivation();
            e.ClearReceivers();
            e.AddReceiver(fieldWithMaxHp);
        }
    }
}
