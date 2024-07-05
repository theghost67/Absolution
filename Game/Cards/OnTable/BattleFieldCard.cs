using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Menus;
using Game.Sleeves;
using Game.Territories;
using Game.Traits;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий карту поля во время сражения, которая может инициировать своё действие на территории сражения.
    /// </summary>
    public class BattleFieldCard : TableFieldCard, IBattleCard, IBattleWeighty
    {
        const int MOXIE_PRIORITY_VALUE = 256;

        public IIdEventBoolAsync<TableFieldAttachArgs> OnFieldTryToAttach => _onFieldTryToAttach;   // before attached to a field (can be canceled)
        public IIdEventVoidAsync<TableFieldAttachArgs> OnFieldPreAttached => _onFieldPreAttached;   // before prev card deleted (field = prev field)
        public IIdEventVoidAsync<TableFieldAttachArgs> OnFieldPostAttached => _onFieldPostAttached; // after prev card deleted (field = new field)

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

        public new BattleFieldCardDrawer Drawer => ((TableObject)this).Drawer as BattleFieldCardDrawer;
        public new BattleTraitListSet Traits => base.Traits as BattleTraitListSet; // has StacksChanged events
        public new BattleField Field => base.Field as BattleField;

        public BattleTerritory Territory => _side.Territory;
        public BattleSide Side
        {
            get => _side;
            set
            {
                if (Field != null)
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
        
        readonly TableEventBool<TableFieldAttachArgs> _onFieldTryToAttach; 
        readonly TableEventVoid<TableFieldAttachArgs> _onFieldPreAttached;  
        readonly TableEventVoid<TableFieldAttachArgs> _onFieldPostAttached; 

        readonly TableEventVoid<ITableEntrySource> _onPreKilled; 
        readonly TableEventVoid<ITableEntrySource> _onPostKilled;  
        readonly TableEventVoid<BattleFieldCard> _onKill;         

        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPreSent;  
        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPostSent;     
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationConfirmed;

        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPreReceived; 
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPostReceived; 

        BattleSide _side;
        BattleArea _area;

        int _turnAge;
        int _phaseAge;
        bool _isBeingKilled;
        bool _isKilled;

        // less or 0 = true
        int _canInitiateCounter; 
        int _canBeKilledCounter;

        public BattleFieldCard(FieldCard data, BattleSide side) 
            : base(data, side.Territory.Transform)
        {
            _onFieldTryToAttach = new TableEventBool<TableFieldAttachArgs>();
            _onFieldPreAttached = new TableEventVoid<TableFieldAttachArgs>();
            _onFieldPostAttached = new TableEventVoid<TableFieldAttachArgs>();
            _onPreKilled = new TableEventVoid<ITableEntrySource>();
            _onPostKilled = new TableEventVoid<ITableEntrySource>();
            _onKill = new TableEventVoid<BattleFieldCard>();
            _onInitiationPreSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationPostSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationConfirmed = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPreReceived = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPostReceived = new TableEventVoid<BattleInitiationRecvArgs>();

            _onFieldTryToAttach.Add(OnFieldTryToAttachBase_TOP, 256);
            _onFieldPreAttached.Add(OnFieldPreAttachedBase_TOP, 256);
            _onFieldPostAttached.Add(OnFieldPostAttachedBase_TOP, 256);
            _onInitiationPreSent.Add(OnInitiationPreSentBase_TOP, 256);
            _onInitiationPostSent.Add(OnInitiationPostSentBase_TOP, 256);
            _onInitiationConfirmed.Add(OnInitiationConfirmedBase_TOP, 256);
            _onInitiationPreReceived.Add(OnInitiationPreReceivedBase_TOP, 256);
            _onInitiationPostReceived.Add(OnInitiationPostReceivedBase_TOP, 256);

            _side = side;
            _area = new BattleArea(this, this);

            Territory.OnStartPhase.Add(OnStartPhase);
            Territory.OnNextPhase.Add(OnNextPhase);
            health.OnPostSet.Add(OnHealthPostSet);
            TryOnInstantiatedAction(GetType(), typeof(BattleFieldCard));
        }
        protected BattleFieldCard(BattleFieldCard src, BattleFieldCardCloneArgs args) : base(src, args)
        {
            _onFieldTryToAttach = (TableEventBool<TableFieldAttachArgs>)src._onFieldTryToAttach.Clone();
            _onFieldPreAttached = (TableEventVoid<TableFieldAttachArgs>)src._onFieldPreAttached.Clone();
            _onFieldPostAttached = (TableEventVoid<TableFieldAttachArgs>)src._onFieldPostAttached.Clone();
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
            TryOnInstantiatedAction(GetType(), typeof(BattleFieldCard));
        }

        public override void Dispose()
        {
            base.Dispose();
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
            Field?.DetatchCard(null);
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleFieldCardCloneArgs cArgs)
                 return new BattleFieldCard(this, cArgs);
            else return null;
        }

        public async UniTask Kill(BattleKillMode mode, ITableEntrySource source)
        {
            if (_isBeingKilled) return;
            if (_isKilled) return;

            _isBeingKilled = true;
            TableEventManager.Add();

            WritePreDeathLog(source);
            if (health > 0)
                await health.SetValue(0, source);

            Drawer?.transform.DOAShake();
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

            DestroyDrawer(false);
            Dispose();

            _isBeingKilled = false;
            TableEventManager.Remove();
        }
        public override async UniTask<bool> CanBeAttachedToField(TableFieldAttachArgs e)
        {
            bool result = await base.CanBeAttachedToField(e);
            if (!result) return result;
            return await _onFieldTryToAttach.InvokeAND(this, e);
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

        protected override async UniTask AttachToFieldInternal(TableFieldAttachArgs e)
        {
            if (e.field != null && e.field.Card != null) return;
            if (e.field == null && IsKilled) return;
            if (e.field == null)
            {
                await base.AttachToFieldInternal(e);
                await SetObserveTargets(false);
                return;
            }

            await _onFieldPreAttached.Invoke(this, e);
            await SetObserveTargets(false);

            await base.AttachToFieldInternal(e);
            _side = Field.Side;

            await SetObserveTargets(true);
            await _onFieldPostAttached.Invoke(this, e);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            BattleFieldCardDrawer drawer = new(this, parent);
            drawer.SetSortingOrder(10, asDefault: true);
            return drawer;
        }

        // used for debug-logging
        protected override async UniTask OnStatPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnPricePreSetBase_TOP(sender, e);

            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            string cardName = card.TableNameDebug;
            string statName = stat.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile($"{cardName}: stats: {statName}: OnPreSet: delta: {e.deltaValue} (by: {sourceName}).");
        }
        protected override async UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnStatPostSetBase_TOP(sender, e);

            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            string cardName = card.TableNameDebug;
            string statName = stat.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile($"{cardName}: stats: {statName}: OnPostSet: delta: {e.totalDeltaValue} (by: {sourceName}).");
        }

        protected virtual UniTask<bool> OnFieldTryToAttachBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardName = card.TableNameDebug;
            string fieldName = e.field?.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile($"{cardName}: field: TryToAttach: field: {fieldName} (by: {sourceName}).");
            return UniTask.FromResult(true);
        }
        protected virtual UniTask OnFieldPreAttachedBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardName = card.TableNameDebug;
            string fieldName = e.field?.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile($"{cardName}: field: PreAttached: field: {fieldName} (by: {sourceName}).");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnFieldPostAttachedBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            BattleTerritory terr = card.Territory;

            string cardName = card.TableNameDebug;
            string fieldName = e.field.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile($"{cardName}: field: PostAttached: field: {fieldName} (by: {sourceName}).");
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnInitiationPreSentBase_TOP(object sender, BattleInitiationSendArgs sArgs)
        {
            string senderName = sArgs.Sender.TableNameDebug;
            string receiversNames = null;
            string manualStr = sArgs.manualAim ? "M" : "";
            foreach (BattleField receiver in sArgs.Receivers)
            {
                if (receiversNames == null)
                    receiversNames = receiver.TableNameDebug;
                else receiversNames += $", {receiver.TableNameDebug}";
            }

            TableConsole.LogToFile($"{senderName}: initiation: OnPreSent: strength: {sArgs.strength}{manualStr}, targets: {receiversNames}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostSentBase_TOP(object sender, BattleInitiationSendArgs sArgs)
        {
            string senderName = sArgs.Sender.TableNameDebug;
            string receiversNames = null;
            string manualStr = sArgs.manualAim ? "M" : "";
            foreach (BattleField receiver in sArgs.Receivers)
            {
                if (receiversNames == null)
                    receiversNames = receiver.TableNameDebug;
                else receiversNames += $", {receiver.TableNameDebug}";
            }

            TableConsole.LogToFile($"{senderName}: initiation: OnPostSent: strength: {sArgs.strength}{manualStr}, targets: {receiversNames}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationConfirmedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableNameDebug;
            string receiverName = rArgs.Receiver.TableNameDebug;

            TableConsole.LogToFile($"{senderName}: initiation: OnConfirmed: strength: {rArgs.strength}, target: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPreReceivedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableNameDebug;
            string receiverName = rArgs.Receiver.TableNameDebug;

            TableConsole.LogToFile($"{senderName}: initiation: OnPreReceived: strength: {rArgs.strength}, target: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostReceivedBase_TOP(object sender, BattleInitiationRecvArgs rArgs)
        {
            string senderName = rArgs.Sender.TableNameDebug;
            string receiverName = rArgs.Receiver.TableNameDebug;

            TableConsole.LogToFile($"{senderName}: initiation: OnPostReceived: strength: {rArgs.strength}, target: {receiverName}.");
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
            await card.Kill(BattleKillMode.Default, e.source);
        }
        async UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card.Field == null || card._isKilled) return;
            if (++card._phaseAge <= MOXIE_PRIORITY_VALUE) return;

            card.Drawer.CreateTextAsSpeech("* СТАРОСТЬ *", Color.red);
            await card.Kill(BattleKillMode.IgnoreEverything, null);
        }
        async UniTask OnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card.Field == null || card._isKilled)
                return;

            card._turnAge++;
            foreach (ITableTraitListElement element in card.Traits)
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
            if (_isKilled || Field == null)
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
            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            if (Drawer != null)
            {
                if (sourceName != null)
                    Menu.WriteLogToCurrent($"{TableName}: смертельное ранение от {sourceName}.");
                else Menu.WriteLogToCurrent($"{TableName}: смертельное ранение.");
            }
            TableConsole.LogToFile($"{TableNameDebug}: OnPreKilled (by: {sourceNameDebug}).");
        }
        void WriteEvadedDeathLog(ITableEntrySource source)
        {
            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            if (Drawer != null)
            {
                if (sourceName != null)
                    Menu.WriteLogToCurrent($"{TableName}: смерть от {sourceName} предотвращена.");
                else Menu.WriteLogToCurrent($"{TableName}: смерть предотвращена.");
            }
            TableConsole.LogToFile($"{TableNameDebug}: evaded being killed (by: {sourceNameDebug})");
        }
        void WritePostDeathLog(ITableEntrySource source)
        {
            string sourceName = source?.TableName;
            string sourceNameDebug = source?.TableNameDebug;

            if (Drawer != null)
            {
                if (sourceName != null)
                    Menu.WriteLogToCurrent($"{TableName}: смерть от {sourceName}.");
                else Menu.WriteLogToCurrent($"{TableName}: смерть.");
            }
            TableConsole.LogToFile($"{TableNameDebug}: OnPostKilled (by: {sourceNameDebug})");
        }
    }
}
