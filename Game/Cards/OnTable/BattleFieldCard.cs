using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
using GreenOne;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий карту поля во время сражения, которая может инициировать своё действие на территории сражения.
    /// </summary>
    public class BattleFieldCard : TableFieldCard, IBattleCard, IBattleWeighty
    {
        const int MOXIE_PRIORITY_VALUE = 256;

        public IIdEventBoolAsync OnFieldTryToAttach => _onFieldTryToAttach;   // before attached to a field (can be canceled)
        public IIdEventVoidAsync OnFieldPreAttached => _onFieldPreAttached;   // before prev card deleted (field = prev field)
        public IIdEventVoidAsync OnFieldPostAttached => _onFieldPostAttached; // after prev card deleted (field = new field)

        public IIdEventVoidAsync<ITableEntrySource> OnPreKilled => _onPreKilled;   // on health dropped to 0 or instant kill attempt [not ignoring events]
        public IIdEventVoidAsync<ITableEntrySource> OnPostKilled => _onPostKilled; // after the card was detatched from a field and before it's destroyed
        public IIdEventVoidAsync<BattleFieldCard> OnKill => _onKill;               // after 'PostKilled' event, raised in initiator (if not dead)

        public IIdEventVoidAsync<BattleInitiationSendArgs> OnInitiationPreSent => _onInitiationPreSent;     // before checking strength && InitiationIsPossible (can increase Str or set 'CanInitiate' to false)
        public IIdEventVoidAsync<BattleInitiationSendArgs> OnInitiationPostSent => _onInitiationPostSent;   // after checked strength && InitiationIsPossible
        public IIdEventVoidAsync<BattleInitiationRecvArgs> OnInitiationConfirmed => _onInitiationConfirmed; // after receiver succesfully received initiation (not dodged/canceled/blocked it etc.)

        public IIdEventVoidAsync<BattleInitiationRecvArgs> OnInitiationPreReceived => _onInitiationPreReceived;   // before taking damage and calling 'OnInitiationConfirmed' from initiator
        public IIdEventVoidAsync<BattleInitiationRecvArgs> OnInitiationPostReceived => _onInitiationPostReceived; // after taking damage and calling 'OnInitiationConfirmed' from initiator

        public int  TurnAge  => _turnAge;
        public int  PhaseAge => _phaseAge;
        public bool IgnoresCards => throw new NotSupportedException(); // TODO: implement (as counters)
        public bool IsKilled => _isKilled;

        public new BattleFieldCardDrawer Drawer => _drawer;
        public new BattleTraitListSet Traits => _traits; // has StacksChanged events
        public new BattleField Field => _field;

        public BattleTerritory Territory => _side.Territory;
        public BattleSide Side
        {
            get => _side;
            set
            {
                if (_field != null)
                    throw new InvalidOperationException($"{nameof(Side)} property cannot be changed directly if card is on a field. Try setting card {nameof(Field)} property instead.");

                if (this is not ITableSleeveCard sCard) 
                    return;

                _side.Sleeve.Remove(sCard);
                _side = value;
                _side.Sleeve.Add(sCard);
            }
        }
        public BattleArea Area => _area;
        public BattleRange Range => BattleRange.normal;
        public BattleWeight Weight => CalculateWeight();

        public bool CanInitiate
        {
            get => _canInitiateCounter >= 0;
            set
            {
                if (value)
                     _canInitiateCounter++;
                else _canInitiateCounter--;
            }
        }
        public bool CanBeKilled
        {
            get => _canBeKilledCounter >= 0;
            set
            {
                if (value)
                     _canBeKilledCounter++;
                else _canBeKilledCounter--;
            }
        }

        public int InitiationOrder => CalculateInitiationOrder();
        public int InitiationPriority => moxie * MOXIE_PRIORITY_VALUE + PhaseAge;
        public bool InitiationIsPossible => CanInitiate && !_isKilled;
        
        readonly TableEventBool _onFieldTryToAttach; 
        readonly TableEventVoid _onFieldPreAttached;  
        readonly TableEventVoid _onFieldPostAttached; 

        readonly TableEventVoid<ITableEntrySource> _onPreKilled; 
        readonly TableEventVoid<ITableEntrySource> _onPostKilled;  
        readonly TableEventVoid<BattleFieldCard> _onKill;         

        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPreSent;  
        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPostSent;     
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationConfirmed;

        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPreReceived; 
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPostReceived; 

        BattleFieldCardDrawer _drawer;
        BattleTraitListSet _traits;
        BattleField _field;
        BattleSide _side;
        BattleArea _area;

        int _turnAge;
        int _phaseAge;
        bool _isBeingKilled;
        bool _isKilled;

        // less or 0 = true
        int _canInitiateCounter; 
        int _canBeKilledCounter;

        public BattleFieldCard(FieldCard data, BattleSide side, bool withDrawer = true, bool fillTraits = true) 
            : base(data, null, withDrawer: false, fillTraits: false)
        {
            _onFieldTryToAttach = new TableEventBool();
            _onFieldPreAttached = new TableEventVoid();
            _onFieldPostAttached = new TableEventVoid();
            _onPreKilled = new TableEventVoid<ITableEntrySource>();
            _onPostKilled = new TableEventVoid<ITableEntrySource>();
            _onKill = new TableEventVoid<BattleFieldCard>();
            _onInitiationPreSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationPostSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationConfirmed = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPreReceived = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPostReceived = new TableEventVoid<BattleInitiationRecvArgs>();

            _onInitiationPreSent.Add(OnInitiationPreSentBase_TOP, 256);
            _onInitiationPostSent.Add(OnInitiationPostSentBase_TOP, 256);
            _onInitiationConfirmed.Add(OnInitiationConfirmedBase_TOP, 256);
            _onInitiationPreReceived.Add(OnInitiationPreReceivedBase_TOP, 256);
            _onInitiationPostReceived.Add(OnInitiationPostReceivedBase_TOP, 256);

            _traits = (BattleTraitListSet)base.Traits;
            _side = side;
            _area = new BattleArea(this, this);

            Territory.OnStartPhase.Add(OnStartPhase);
            Territory.OnNextPhase.Add(OnNextPhase);
            health.OnPostSet.Add(OnHealthPostSet);

            if (fillTraits)
                _traits.AdjustStacksInRange(data.traits, this);

            if (withDrawer)
                CreateDrawer(side.Territory.transform);
        }
        protected BattleFieldCard(BattleFieldCard src, BattleFieldCardCloneArgs args) : base(src, args)
        {
            _onFieldTryToAttach = (TableEventBool)src._onFieldTryToAttach.Clone();
            _onFieldPreAttached = (TableEventVoid)src._onFieldPreAttached.Clone();
            _onFieldPostAttached = (TableEventVoid)src._onFieldPostAttached.Clone();
            _onPreKilled = (TableEventVoid<ITableEntrySource>)src._onPreKilled.Clone();
            _onPostKilled = (TableEventVoid<ITableEntrySource>)src._onPostKilled.Clone();
            _onKill = (TableEventVoid<BattleFieldCard>)src._onKill.Clone();
            _onInitiationPreSent = (TableEventVoid<BattleInitiationSendArgs>)src._onInitiationPreSent.Clone();
            _onInitiationPostSent = (TableEventVoid<BattleInitiationSendArgs>)src._onInitiationPostSent.Clone();
            _onInitiationConfirmed = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationConfirmed.Clone();
            _onInitiationPreReceived = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationPreReceived.Clone();
            _onInitiationPostReceived = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationPostReceived.Clone();

            _phaseAge = src._phaseAge;
            _isKilled = src._isKilled;
            _canInitiateCounter = src._canInitiateCounter;
            _canBeKilledCounter = src._canBeKilledCounter;

            BattleAreaCloneArgs areaCArgs = new(this, this, args.terrCArgs);
            _side = args.srcCardSideClone;
            _area = (BattleArea)src._area.Clone(areaCArgs);

            _field = (BattleField)base.Field;
            args.TryOnClonedAction(src.GetType(), typeof(BattleFieldCard));
        }

        public override void Dispose()
        {
            base.Dispose();

            _traits.Dispose();
            _area.Dispose();

            OnFieldTryToAttach.Dispose();
            OnFieldPreAttached.Dispose();
            OnFieldPostAttached.Dispose();
            OnPreKilled.Dispose();
            OnPostKilled.Dispose();
            OnKill.Dispose();
            OnInitiationPreSent.Dispose();
            OnInitiationPostSent.Dispose();
            OnInitiationConfirmed.Dispose();
            OnInitiationPreReceived.Dispose();
            OnInitiationPostReceived.Dispose();

            Territory.OnStartPhase.Remove(OnStartPhase);
            Territory.OnNextPhase.Remove(OnNextPhase);
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleFieldCardCloneArgs cArgs)
                 return new BattleFieldCard(this, cArgs);
            else return null;
        }

        public async UniTask Kill(ITableEntrySource source, BattleKillMode mode = default)
        {
            if (_isBeingKilled) return;
            if (_isKilled) return;

            _isBeingKilled = true;
            TableEventManager.Add();

            WritePreDeathLog(source);
            if (health > 0)
                await health.SetValueAbs(0, source);

            _drawer?.transform.DOAShake();
            await _onPreKilled.Invoke(this, source);

            if ((!mode.HasFlag(BattleKillMode.IgnoreCanBeKilled) && !CanBeKilled) ||
                (!mode.HasFlag(BattleKillMode.IgnoreHealthRestore) && health > 0))
            {
                WriteEvadedDeathLog(source);
                return;
            }

            WritePostDeathLog(source);
            _isKilled = true;

            await _onPostKilled.Invoke(this, source);
            if (source is BattleFieldCard bfCard && !bfCard.IsKilled)
                await bfCard._onKill.Invoke(bfCard, this);

            await AttachToAnotherField(null);
            DestroyDrawer(false);
            Dispose();

            _isBeingKilled = false;
            TableEventManager.Remove();
        }
        public override async UniTask<bool> CanBeAttachedToField(TableField field)
        {
            bool result = await _onFieldTryToAttach.InvokeAND(this, EventArgs.Empty);
            if (!result && field != null)
                field.Drawer.CreateTextAsSpeech("Смена поля\nотменена", Color.red);
            return result;
        }

        public BattleInitiationSendArgs CreateInitiation()
        {
            int strengthPerTarget = (strength / (float)Range.splash.targetsCount).Ceiling();
            return new BattleInitiationSendArgs(this, strengthPerTarget, topPriority: false, manualAim: Side.isMe);
        }

        protected override TableTraitListSet TraitListSetCreator()
        {
            return new BattleTraitListSet(this);
        }
        protected override TableTraitListSet TraitListSetCloner(TableTraitListSet src, TableFieldCardCloneArgs args)
        {
            BattleFieldCardCloneArgs argsCast = (BattleFieldCardCloneArgs)args;
            BattleTraitListSetCloneArgs setArgs = new(this, argsCast.terrCArgs);
            return (TableTraitListSet)src.Clone(setArgs);
        }
        protected override void TraitListSetSetter(TableTraitListSet value)
        {
            base.TraitListSetSetter(value);
            _traits = (BattleTraitListSet)value;
        }

        protected override async UniTask FieldPropSetter(TableField value)
        {
            if (value != null && value.Card != null) return;
            if (value == null && IsKilled) return;
            if (!await CanBeAttachedToField(value)) return;

            if (value == null)
            {
                await _field.DetatchCard();
                await SetObserveTargets(false);
                FieldBaseSetter(null);
                return;
            }

            await _onFieldPreAttached.Invoke(this, EventArgs.Empty);
            await SetObserveTargets(false);

            FieldBaseSetter(value);
            _side = _field.Side;

            await _field.AttachCard(this);
            await SetObserveTargets(true);
            await _onFieldPostAttached.Invoke(this, EventArgs.Empty);
        }
        protected override void FieldBaseSetter(TableField value)
        {
            base.FieldBaseSetter(value);
            _field = (BattleField)value;
        }

        protected override void DrawerSetter(TableCardDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattleFieldCardDrawer)value;
        }
        protected override TableCardDrawer DrawerCreator(Transform parent)
        {
            BattleFieldCardDrawer drawer = new(this, parent);
            drawer.SetSortingOrder(10, asDefault: true);
            return drawer;
        }

        // used for logging
        protected override async UniTask OnPricePreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnPricePreSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: цена: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: цена: попытка изменения на {e.deltaValue}.");
        }
        protected override async UniTask OnHealthPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnHealthPreSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: здоровье: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: здоровье: попытка изменения на {e.deltaValue} ед.");
        }
        protected override async UniTask OnStrengthPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnStrengthPreSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: сила: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: сила: попытка изменения на {e.deltaValue} ед.");
        }
        protected override async UniTask OnMoxiePreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnMoxiePreSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: инициатива: попытка изменения на {e.deltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: инициатива: попытка изменения на {e.deltaValue} ед.");
        }

        protected override async UniTask OnPricePostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnPricePostSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: цена: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: цена: изменение на {e.totalDeltaValue} ед.");
        }
        protected override async UniTask OnHealthPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnHealthPostSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: здоровье: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: здоровье: изменение на {e.totalDeltaValue} ед.");
        }
        protected override async UniTask OnStrengthPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnStrengthPostSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: сила: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: сила: изменение на {e.totalDeltaValue} ед.");
        }
        protected override async UniTask OnMoxiePostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnMoxiePostSetBase_TOP(sender, e);
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;

            if (e.source != null)
                 card.Territory.WriteLogForDebug($"{card.TableName}: инициатива: изменение на {e.totalDeltaValue} ед. от {e.source.TableName}.");
            else card.Territory.WriteLogForDebug($"{card.TableName}: инициатива: изменение на {e.totalDeltaValue} ед.");
        }

        protected virtual UniTask OnInitiationPreSentBase_TOP(object sender, BattleInitiationSendArgs sArgs)
        {
            string senderName = sArgs.Sender.TableName;
            string receiversName = null;
            foreach (BattleField receiver in sArgs.Receivers)
            {
                if (receiversName == null)
                     receiversName = receiver.TableName;
                else receiversName += $" {receiver.TableName}";
            }
            sArgs.Sender.Territory.WriteLogForDebug($"{senderName}: инициация: пред-отправление, сила {sArgs.strength} ед., цели: {receiversName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostSentBase_TOP(object sender, BattleInitiationSendArgs sArgs)
        {
            string senderName = sArgs.Sender.TableName;
            string receiversName = null;
            foreach (BattleField receiver in sArgs.Receivers)
            {
                if (receiversName == null)
                    receiversName = receiver.TableName;
                else receiversName += $" {receiver.TableName}";
            }
            sArgs.Sender.Territory.WriteLogForDebug($"{senderName}: инициация: пост-отправление, сила {sArgs.strength} ед., цели: {receiversName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationConfirmedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableName;
            string receiverName = rArgs.Receiver.TableName;
            rArgs.Sender.Territory.WriteLogForDebug($"{senderName}: инициация: подтверждение, сила {rArgs.strength} ед., цель: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPreReceivedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableName;
            string receiverName = rArgs.Receiver.TableName;
            rArgs.Sender.Territory.WriteLogForDebug($"{senderName}: инициация: пред-получение, сила {rArgs.strength} ед., получатель: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostReceivedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableName;
            string receiverName = rArgs.Receiver.TableName;
            rArgs.Sender.Territory.WriteLogForDebug($"{senderName}: инициация: пост-получение, сила {rArgs.strength} ед., получатель: {receiverName}.");
            return UniTask.CompletedTask;
        }
        // ----

        async UniTask SetObserveTargets(bool value)
        {
            await _area.SetObserveTargets(value);
            foreach (IBattleTraitListElement element in Traits)
                await element.Trait.Area.SetObserveTargets(value);
        }
        async UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            if (e.newStatValue > 0) return;
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            await card.Kill(e.source);
        }
        async UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card._field == null || card._isKilled) return;
            if (++card._phaseAge <= MOXIE_PRIORITY_VALUE) return;

            card.Drawer.CreateTextAsSpeech("* СТАРОСТЬ *", Color.red);
            await card.Kill(null, BattleKillMode.IgnoreEverything);
        }
        async UniTask OnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card._field == null || card._isKilled)
                return;

            card._turnAge++;
            foreach (ITableTraitListElement element in card._traits)
            {
                TableTraitStorage storage = element.Trait.Storage;
                storage.turnsDelay--;
                storage.turnsPassed++;
            }
        }

        int CalculateInitiationOrder()
        {
            List<BattleFieldCard> cards = Side.Territory.Fields().WithCard().Select(f => f.Card).ToList();
            cards.Sort((x, y) => -x.InitiationPriority.CompareTo(y.InitiationPriority)); // reversed
            return cards.IndexOf(this) + 1;
        }
        BattleWeight CalculateWeight()
        {
            if (_isKilled || _field == null)
                return BattleWeight.none;

            float absWeight = health + strength;
            float relWeight = 0;

            foreach (IBattleTraitListElement element in Traits)
            {
                BattleWeight weight = element.Trait.Weight;
                absWeight += weight.absolute;
                relWeight += weight.relative;
            }
            return new BattleWeight(absWeight, relWeight);
        }

        void WritePreDeathLog(ITableEntrySource source)
        {
            if (source != null)
                 Territory.WriteLog($"{TableName}: смертельное ранение от {source.TableName}.");
            else Territory.WriteLog($"{TableName}: смертельное ранение.");
        }
        void WriteEvadedDeathLog(ITableEntrySource source)
        {
            if (source != null)
                 Territory.WriteLog($"{TableName}: смерть от {source.TableName} предотвращена.");
            else Territory.WriteLog($"{TableName}: смерть предотвращена.");
        }
        void WritePostDeathLog(ITableEntrySource source)
        {
            if (source != null)
                 Territory.WriteLog($"{TableName}: смерть от {source.TableName}.");
            else Territory.WriteLog($"{TableName}: смерть.");
        }
    }
}
