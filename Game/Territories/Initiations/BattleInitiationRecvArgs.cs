﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using System;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий получаемую инициацию. Позволяет изменять локальные данные инициации перед её действием.
    /// </summary>
    public class BattleInitiationRecvArgs
    {
        public event EventHandler<BattleField> OnReceiverChanged;
        public event EventHandler<int> OnStrengthChanged; // called in strength.OnPostSet

        public BattleFieldCard Sender { get; }
        public BattleField Receiver 
        {
            get => _receiver; 
            set
            {
                if (value == null)
                    throw new ArgumentNullException($"Receiver must have not null reference (to cancel initiation or remove this field from targets, handle it in {nameof(Sender.OnInitiationPreSent)})");

                _receiver = value;
                _receiverChanged = true;
                OnReceiverChanged?.Invoke(this, value);
            }
        }
        public bool ReceiverChanged => _receiverChanged;

        public readonly TableStat strength; // does not matter if ReceiverChanged (will redirect attack, creating a new RecvArgs from new receiver)
        public readonly BattleField receiverDefault;
        public bool handled;

        BattleField _receiver;
        bool _receiverChanged;

        public BattleInitiationRecvArgs(BattleField receiver, BattleInitiationSendArgs sArgs)
        {
            Sender = sArgs.Sender;
            strength = new TableStat(nameof(strength), this, sArgs.strength);
            strength.OnPostSet.Add(OnStrengthPostSet);
            receiverDefault = receiver;
            _receiver = receiver;
        }
        UniTask OnStrengthPostSet(object sender, TableStat.PostSetArgs e)
        {
            OnStrengthChanged?.Invoke(this, e.newStatValue);
            return UniTask.CompletedTask;
        }
    }
}
