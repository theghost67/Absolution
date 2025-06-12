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
        public BattleFieldCard Sender => SenderArgs.Sender;
        public TableStat Strength { get; } // does not matter if ReceiverChanged (will redirect attack, creating a new RecvArgs from new receiver)
        public BattleField ReceiverField 
        {
            get => _receiverField; 
            set
            {
                if (value == null)
                    throw new ArgumentNullException($"{nameof(ReceiverField)} must have not null reference (to cancel initiation or remove this field from targets, handle it in {nameof(Sender.OnInitiationPreSent)})");
                if (value == _receiverField)
                    return;

                _receiverPrev = _receiverField;
                _receiverField = value;
                OnReceiverChanged?.Invoke(this, value);
            }
        }
        public BattleFieldCard ReceiverCard => _receiverField.Card; // could be null or killed
        public bool IgnoresCard { get; set; }

        internal BattleField ReceiverPrev => _receiverPrev; // internal use

        public readonly TableEventVoid OnPreReceived;
        public readonly TableEventVoid OnPostReceived;

        public bool handled;

        BattleField _receiverField;
        BattleField _receiverPrev;

        public BattleInitiationRecvArgs(BattleField receiver, BattleInitiationSendArgs sArgs)
        {
            SenderArgs = sArgs;

            Strength = new TableStat("strength", this, sArgs.Strength);
            Strength.OnPostSet.Add(null, OnStrengthPostSet);
            ReceiverField = receiver;

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
