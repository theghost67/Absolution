using Cysharp.Threading.Tasks;
using Game.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий отправляемую инициацию. Позволяет изменять общие данные инициации перед её действием.
    /// </summary>
    public sealed class BattleInitiationSendArgs : IComparable<BattleInitiationSendArgs>
    {
        public event EventHandler<BattleField> OnReceiverAdded;
        public event EventHandler<BattleField> OnReceiverRemoved;
        public event EventHandler<int> OnStrengthChanged; // called in strength.OnPostSet

        public BattleFieldCard Sender => _sender;
        public TableStat Strength => _strength;
        public int Priority => topPriority ? int.MaxValue : _sender.InitiationPriority;
        public IReadOnlyList<BattleField> Receivers => _receivers;

        public readonly TableEventVoid OnPreSent;
        public readonly TableEventVoid OnPostSent;
        public readonly TableEventVoid<BattleInitiationRecvArgs> OnConfirmed;

        public bool manualAim;
        public bool topPriority;
        public bool handled;

        readonly TableStat _strength;
        readonly BattleFieldCard _sender;
        readonly List<BattleField> _receivers;

        // if receivers.Count == 0, it will use sender targets
        public BattleInitiationSendArgs(BattleFieldCard sender, int strength, bool topPriority, bool manualAim, params BattleField[] receivers)
        {
            if (sender.Field == null) 
                throw new Exception("Initiation sender must have a field.");

            this._strength = new TableStat("strength", this, strength);
            this._strength.OnPostSet.Add("base", OnStrengthPostSet);

            this.manualAim = manualAim;
            this.topPriority = topPriority;

            _sender = sender;
            _receivers = new List<BattleField>(receivers);

            OnPreSent = new TableEventVoid();
            OnPostSent = new TableEventVoid();
            OnConfirmed = new TableEventVoid<BattleInitiationRecvArgs>();
        }
        public int CompareTo(BattleInitiationSendArgs other)
        {
            return Priority - other.Priority;
        }

        public void AddReceiver(BattleField field)
        {
            _receivers.Add(field);
            OnReceiverAdded?.Invoke(this, field);
        }
        public void RemoveReceiver(BattleField field)
        {
            _receivers.Remove(field);
            OnReceiverRemoved?.Invoke(this, field);
        }
        public void ClearReceivers()
        {
            for (int i = _receivers.Count - 1; i >= 0; i--)
            {
                BattleField field = _receivers[i];
                _receivers.RemoveAt(i);
                OnReceiverRemoved?.Invoke(this, field);
            }
        }
        public async UniTask SelectReceivers()
        {
            if (_receivers.Count != 0) return;
            if (_sender.Drawer == null || _sender.Side.ai.UsesAiming || !manualAim)
                Sender.Area.SelectTargetsByWeight();
            else
            {
                ShowAimMode();
                bool isAimed = false;
                Sender.Area.StartTargetAiming(f => true, f => isAimed = true, canCancel: false);
                while (!isAimed) 
                    await UniTask.Yield();
                HideAnimMode();
            }

            foreach (BattleField target in Sender.Area.AimedTargetSplash())
                _receivers.Add(target);
        }

        UniTask OnStrengthPostSet(object sender, TableStat.PostSetArgs e)
        {
            OnStrengthChanged?.Invoke(this, e.newStatValue);
            return UniTask.CompletedTask;
        }

        // TODO: show animations etc.
        void ShowAimMode()
        {

        }
        void HideAnimMode()
        {

        }
    }
}
