using Cysharp.Threading.Tasks;
using Game.Cards;
using System;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий получаемую инициацию. Позволяет изменять локальные данные инициации перед её действием.
    /// </summary>
    public sealed class BattleInitiationRecvArgs
    {
        public event EventHandler<BattleField> OnReceiverChanged;
        public event EventHandler<int> OnStrengthChanged; // called in strength.OnPostSet

        public BattleInitiationSendArgs SenderArgs { get; }
        public BattleFieldCard Sender { get; }

        public BattleField Receiver 
        {
            get => _receiver; 
            set
            {
                if (value == null)
                    throw new ArgumentNullException($"Receiver must have not null reference (to cancel initiation or remove this field from targets, handle it in {nameof(Sender.OnInitiationPreSent)})");
                if (value == _receiver)
                    return;

                _receiverPrev = _receiver;
                _receiver = value;
                OnReceiverChanged?.Invoke(this, value);
            }
        }
        public BattleField ReceiverPrev => _receiverPrev;

        public readonly TableEventVoid OnPreReceived;
        public readonly TableEventVoid OnPostReceived;

        public readonly TableStat strength; // does not matter if ReceiverChanged (will redirect attack, creating a new RecvArgs from new receiver)
        public bool handled;

        BattleField _receiver;
        BattleField _receiverPrev;

        public BattleInitiationRecvArgs(BattleField receiver, BattleInitiationSendArgs sArgs)
        {
            SenderArgs = sArgs;
            Sender = sArgs.Sender;

            strength = new TableStat("strength", this, sArgs.strength);
            strength.OnPostSet.Add(null, OnStrengthPostSet);
            Receiver = receiver;

            OnPreReceived = new TableEventVoid();
            OnPostReceived = new TableEventVoid();
        }
        UniTask OnStrengthPostSet(object sender, TableStat.PostSetArgs e)
        {
            OnStrengthChanged?.Invoke(this, e.newStatValue);
            return UniTask.CompletedTask;
        }
    }
}
