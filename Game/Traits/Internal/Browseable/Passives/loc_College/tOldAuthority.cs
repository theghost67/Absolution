using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOldAuthority : PassiveTrait
    {
        const string ID = "old_authority";
        const int PRIORITY = 8;

        public tOldAuthority() : base(ID)
        {
            name = "Пожилой авторитет";
            desc = "Слушай, зачем нам махаться друг с другом? Пойдём попьём чаю.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tOldAuthority(tOldAuthority other) : base(other) { }
        public override object Clone() => new tOldAuthority(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед любой инициацией владельца (П{PRIORITY})",
                    $"отменяет инициацию на цель, если у неё есть навык <i>{name}</i>."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs sArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            bool firstActivation = true;
            for (int i = 0; i < sArgs.Receivers.Count; i++)
            {
                BattleField receiver = sArgs.Receivers[i];
                if (receiver.Card == null || receiver.Card.Traits.Passive(ID) == null) continue;
                if (firstActivation)
                {
                    await trait.AnimActivation();
                    firstActivation = false;
                }
                sArgs.RemoveReceiver(receiver);
                i--;
            }
        }
    }
}
