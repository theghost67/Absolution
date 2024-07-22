using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPigeonFright : PassiveTrait
    {
        const string ID = "pigeon_fright";
        const int PRIORITY = 6;
        const string SPAWN_CARD_ID = "pigeon_litter";

        public tPigeonFright() : base(ID)
        {
            name = "Голубиный испуг";
            desc = $"- Испугались?\n- Курлык.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.ownerDouble);
            frequency = 0.12f;
        }
        protected tPigeonFright(tPigeonFright other) : base(other) { }
        public override object Clone() => new tPigeonFright(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(SPAWN_CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед получением атакующей инициации владельцем (П{PRIORITY})",
                    $"если есть свободное поле слева или справа от владельца, тратит один заряд и перемещается на это поле, оставляя на прошлом месте карту <i>{cardName}</i>.")
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 4 * Mathf.Pow(stacks - 1, 2);
        }
        public override UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            if (!e.isInBattle)
                return UniTask.CompletedTask;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;
            BattleFieldCard owner = trait.Owner;

            if (trait.WasAdded(e)) owner.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived, PRIORITY);
            if (trait.WasRemoved(e)) owner.OnInitiationPreReceived.Remove(trait.GuidStr);

            return UniTask.CompletedTask;
        }

        static async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            BattleField[] fields = trait.Area.PossibleTargets().WithoutCard().ToArray();
            if (fields.Length == 0) return;

            FieldCard spawnCardData = CardBrowser.NewField(SPAWN_CARD_ID);
            BattleField prevField = owner.Field;
            e.Receiver = prevField;

            await trait.AnimActivation();
            await owner.TryAttachToField(fields.First(), trait);

            await owner.Territory.PlaceFieldCard(spawnCardData, prevField, trait.Side);
            await trait.AdjustStacks(-1, e.Sender);
        }
    }
}
