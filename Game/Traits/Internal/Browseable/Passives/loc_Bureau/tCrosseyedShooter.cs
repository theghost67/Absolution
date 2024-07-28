using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using Unity.Mathematics;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCrosseyedShooter : PassiveTrait
    {
        const string ID = "crosseyed_shooter";
        const int PRIORITY = 4;

        public tCrosseyedShooter() : base(ID)
        {
            name = "Косоглазый стрелок";
            desc = "Я попал! Я попал в него! Это ведь он?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tCrosseyedShooter(tCrosseyedShooter other) : base(other) { }
        public override object Clone() => new tCrosseyedShooter(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед атакой владельца (П{PRIORITY})",
                    $"Атакует вражеские поля слева/справа по очереди (сначала левое, потом правое, затем снова левое и так далее)."),
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

            await trait.AnimActivation();
            int2 pos = owner.Field.Opposite.pos;
            e.ClearReceivers();

            if (trait.Storage.ContainsKey(ID))
            {
                pos.x++; // opposite right
                trait.Storage.Remove(ID);
            }
            else
            {
                pos.x--; // opposite left
                trait.Storage.Add(ID, null);
            }

            BattleField field = owner.Territory.Field(pos);
            if (field != null) e.AddReceiver(field);
        }
    }
}
