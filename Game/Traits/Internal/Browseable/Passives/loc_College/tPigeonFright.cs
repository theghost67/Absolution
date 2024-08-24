using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

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
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tPigeonFright() : base(ID)
        {
            name = "Голубиный испуг";
            desc = $"- Испугались?\n- Курлык.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
        }
        protected tPigeonFright(tPigeonFright other) : base(other) { }
        public override object Clone() => new tPigeonFright(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(SPAWN_CARD_ID).name;
            return $"<color>Перед атакой на владельца (П{PRIORITY})</color>\nЕсли есть свободное поле слева или справа от владельца, " +
                   $"тратит один заряд и перемещается на это поле, оставляя на прошлом месте карту <u>{cardName}</u>.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection() 
            { new CardDescriptiveArgs(SPAWN_CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.25f);
        }
        public override UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            if (!e.isInBattle)
                return UniTask.CompletedTask;

            IBattleTrait trait = (IBattleTrait)e.trait;
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
            if (owner.Field == null) return;

            BattleField[] fields = trait.Territory.Fields(owner.Field.pos, _range).WithoutCard().ToArray();
            if (fields.Length == 0) return;

            FieldCard spawnCardData = CardBrowser.NewField(SPAWN_CARD_ID);
            BattleField prevField = owner.Field;
            e.ReceiverField = prevField;

            await trait.AnimActivation();
            await trait.AdjustStacks(-1, e.Sender);

            await owner.TryAttachToField(fields.First(), trait);
            if (prevField.Card == null)
                await owner.Territory.PlaceFieldCard(spawnCardData, prevField, trait.Side);
        }
    }
}
