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

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед совершением атаки владельца</color>\nОтменяет атаку на цель, если у неё есть навык <i>{name}</i>.";
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

            bool firstActivation = true;
            for (int i = 0; i < e.Receivers.Count; i++)
            {
                BattleField receiver = e.Receivers[i];
                if (receiver.Card == null || receiver.Card.Traits.Passive(ID) == null) continue;
                if (firstActivation)
                {
                    await trait.AnimActivation();
                    firstActivation = false;
                }
                e.RemoveReceiver(receiver);
                i--;
            }
        }
    }
}
