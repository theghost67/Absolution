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
    public class BattleFieldCard : TableFieldCard, IBattleCard, IBattleFighter, IBattleKillable
    {
        const int MOXIE_PRIORITY_VALUE = 256;

        public IIdEventBoolAsync<TableFieldAttachArgs> OnFieldTryToAttach => _onFieldTryToAttach;   // before attached to a field (can be canceled)
        public ITableEventVoid<TableFieldAttachArgs> OnFieldPreAttached => _onFieldPreAttached;   // before prev card deleted (field = prev field)
        public ITableEventVoid<TableFieldAttachArgs> OnFieldPostAttached => _onFieldPostAttached; // after prev card deleted (field = new field)

        public ITableEventVoid<BattleKillAttemptArgs> OnPreKilled => _onPreKilled;                 // on health dropped to 0 or instant kill attempt [not ignoring events]
        public ITableEventVoid<BattleKillAttemptArgs> OnPostKilled => _onPostKilled;               // after the card was detatched from a field and before it's destroyed
        public ITableEventVoid<BattleKillAttemptArgs> OnEvadedBeingKilled => _onEvadedBeingKilled; // if card restored health or became unkillable
        public ITableEventVoid<BattleKillConfirmArgs> OnKillConfirmed => _onKillConfirmed;         // after 'PostKilled' event, raised in initiator (if not dead)

        public ITableEventVoid<BattleInitiationSendArgs> OnInitiationPreSent => _onInitiationPreSent;     // before checking strength && InitiationIsPossible (can increase strength or set 'CanInitiate' to false)
        public ITableEventVoid<BattleInitiationSendArgs> OnInitiationPostSent => _onInitiationPostSent;   // after checked strength && InitiationIsPossible
        public ITableEventVoid<BattleInitiationRecvArgs> OnInitiationConfirmed => _onInitiationConfirmed; // after receiver succesfully received initiation (not dodged/canceled/blocked it etc.)

        public ITableEventVoid<BattleInitiationRecvArgs> OnInitiationPreReceived => _onInitiationPreReceived;   // before taking damage and calling 'OnInitiationConfirmed' from initiator
        public ITableEventVoid<BattleInitiationRecvArgs> OnInitiationPostReceived => _onInitiationPostReceived; // after taking damage and calling 'OnInitiationConfirmed' from initiator

        public int PhaseAge => _phaseAge;
        public bool IsKilled => _isKilled;

        public new BattleFieldCardDrawer Drawer => ((TableObject)this).Drawer as BattleFieldCardDrawer;
        public new BattleTraitListSet Traits => base.Traits as BattleTraitListSet; // has StacksChanged events
        public new BattleField Field => base.Field as BattleField;
        public new BattleField LastField => base.LastField as BattleField;

        public BattleTerritory Territory => _side.Territory;
        public BattleSide Side => _side;
        public BattleArea Area => _area;
        public BattleRange Range => BattleRange.normal;

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
        public int InitiationPriority => Moxie * MOXIE_PRIORITY_VALUE + PhaseAge;
        public bool InitiationIsPossible => CanInitiate && !_isKilled;

        readonly TableEventBool<TableFieldAttachArgs> _onFieldTryToAttach; 
        readonly TableEventVoid<TableFieldAttachArgs> _onFieldPreAttached;  
        readonly TableEventVoid<TableFieldAttachArgs> _onFieldPostAttached; 

        readonly TableEventVoid<BattleKillAttemptArgs> _onPreKilled; 
        readonly TableEventVoid<BattleKillAttemptArgs> _onPostKilled;
        readonly TableEventVoid<BattleKillAttemptArgs> _onEvadedBeingKilled;
        readonly TableEventVoid<BattleKillConfirmArgs> _onKillConfirmed;         

        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPreSent;  
        readonly TableEventVoid<BattleInitiationSendArgs> _onInitiationPostSent;     
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationConfirmed;

        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPreReceived; 
        readonly TableEventVoid<BattleInitiationRecvArgs> _onInitiationPostReceived;
        readonly string _eventsGuid;

        BattleSide _side;
        BattleArea _area;

        int _phaseAge;
        bool _killBlock;
        bool _isKilled;

        // less or 0 = true
        int _canInitiateCounter; 
        int _canBeKilledCounter;

        public BattleFieldCard(FieldCard data, BattleSide side) : base(data, side.Territory.Transform)
        {
            _onFieldTryToAttach = new TableEventBool<TableFieldAttachArgs>();
            _onFieldPreAttached = new TableEventVoid<TableFieldAttachArgs>();
            _onFieldPostAttached = new TableEventVoid<TableFieldAttachArgs>();
            _onPreKilled = new TableEventVoid<BattleKillAttemptArgs>();
            _onPostKilled = new TableEventVoid<BattleKillAttemptArgs>();
            _onEvadedBeingKilled = new TableEventVoid<BattleKillAttemptArgs>();
            _onKillConfirmed = new TableEventVoid<BattleKillConfirmArgs>();
            _onInitiationPreSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationPostSent = new TableEventVoid<BattleInitiationSendArgs>();
            _onInitiationConfirmed = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPreReceived = new TableEventVoid<BattleInitiationRecvArgs>();
            _onInitiationPostReceived = new TableEventVoid<BattleInitiationRecvArgs>();
            _eventsGuid = this.GuidGen(3);

            _onPreKilled.Add(_eventsGuid, OnPreKilledBase_TOP);
            _onPostKilled.Add(_eventsGuid, OnPostKilledBase_TOP);
            _onEvadedBeingKilled.Add(_eventsGuid, OnEvadedBeingKilledBase_TOP);
            _onKillConfirmed.Add(_eventsGuid, OnKillConfirmedBase_TOP);

            _onFieldTryToAttach.Add(_eventsGuid, OnFieldTryToAttachBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onFieldPreAttached.Add(_eventsGuid, OnFieldPreAttachedBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onFieldPostAttached.Add(_eventsGuid, OnFieldPostAttachedBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onInitiationPreSent.Add(_eventsGuid, OnInitiationPreSentBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onInitiationPostSent.Add(_eventsGuid, OnInitiationPostSentBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onInitiationConfirmed.Add(_eventsGuid, OnInitiationConfirmedBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onInitiationPreReceived.Add(_eventsGuid, OnInitiationPreReceivedBase_TOP, TableEventVoid.TOP_PRIORITY);
            _onInitiationPostReceived.Add(_eventsGuid, OnInitiationPostReceivedBase_TOP, TableEventVoid.TOP_PRIORITY);

            _side = side;
            _area = new BattleArea(this, this);

            Territory.OnStartPhase.Add(_eventsGuid, OnStartPhase);
            Territory.OnNextPhase.Add(_eventsGuid, OnNextPhase);
            Health.OnPostSet.Add(_eventsGuid, OnHealthPostSet);
            TryOnInstantiatedAction(GetType(), typeof(BattleFieldCard));
        }
        protected BattleFieldCard(BattleFieldCard src, BattleFieldCardCloneArgs args) : base(src, args)
        {
            _onFieldTryToAttach = (TableEventBool<TableFieldAttachArgs>)src._onFieldTryToAttach.Clone();
            _onFieldPreAttached = (TableEventVoid<TableFieldAttachArgs>)src._onFieldPreAttached.Clone();
            _onFieldPostAttached = (TableEventVoid<TableFieldAttachArgs>)src._onFieldPostAttached.Clone();

            _onPreKilled = (TableEventVoid<BattleKillAttemptArgs>)src._onPreKilled.Clone();
            _onPostKilled = (TableEventVoid<BattleKillAttemptArgs>)src._onPostKilled.Clone();
            _onEvadedBeingKilled = (TableEventVoid<BattleKillAttemptArgs>) src._onEvadedBeingKilled.Clone();
            _onKillConfirmed = (TableEventVoid<BattleKillConfirmArgs>)src._onKillConfirmed.Clone();

            _onInitiationPreSent = (TableEventVoid<BattleInitiationSendArgs>)src._onInitiationPreSent.Clone();
            _onInitiationPostSent = (TableEventVoid<BattleInitiationSendArgs>)src._onInitiationPostSent.Clone();
            _onInitiationConfirmed = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationConfirmed.Clone();
            _onInitiationPreReceived = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationPreReceived.Clone();
            _onInitiationPostReceived = (TableEventVoid<BattleInitiationRecvArgs>)src._onInitiationPostReceived.Clone();
            _eventsGuid = (string)src._eventsGuid.Clone();

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

            _onFieldTryToAttach.Dispose();
            _onFieldPreAttached.Dispose();
            _onFieldPostAttached.Dispose();
            _onPreKilled.Dispose();
            _onPostKilled.Dispose();
            _onKillConfirmed.Dispose();
            _onInitiationPreSent.Dispose();
            _onInitiationPostSent.Dispose();
            _onInitiationConfirmed.Dispose();
            _onInitiationPreReceived.Dispose();
            _onInitiationPostReceived.Dispose();

            Territory.OnStartPhase.Remove(_eventsGuid);
            Territory.OnNextPhase.Remove(_eventsGuid);
            Field?.DetatchCard(null);
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleFieldCardCloneArgs cArgs)
                 return new BattleFieldCard(this, cArgs);
            else return null;
        }

        public async UniTask TryAttachToSideSleeve(BattleSide side, ITableEntrySource source)
        {
            if (this is not ITableSleeveCard sCard)
                throw new InvalidCastException($"Card should implement {nameof(ITableSleeveCard)} interface to have the ability to be attached to sleeves.");
            if (Field != null)
                await TryAttachToField(null, source);
            if (Field != null)
                return;
            if (_side.Sleeve.Contains(sCard)) 
                return;

            _side.Sleeve.Remove(sCard);
            _side = side;
            _side.Sleeve.Add(sCard);
            await UniTask.Delay((int)(ITableSleeveCard.PULL_DURATION * 1000));
        }
        public async UniTask TryKill(BattleKillMode mode, ITableEntrySource source)
        {
            if (_killBlock) return;
            if (_isKilled) return;

            _killBlock = true;
            if (Health > 0)
                await Health.SetValue(0, source);
            _killBlock = false;

            BattleKillAttemptArgs args = new(this, Field, mode, source);
            if (args.handled)
            {
                await _onEvadedBeingKilled.Invoke(this, args);
                return;
            }

            Drawer?.transform.DOAShake();
            await _onPreKilled.Invoke(this, args);
            if (_isKilled) return; // can be killed inside of the event

            args = new(this, Field, mode, source);
            if (args.handled)
            {
                await _onEvadedBeingKilled.Invoke(this, args);
                return;
            }

            _isKilled = true;
            await _onPostKilled.Invoke(this, args);
            await AttachToFieldInternal(new TableFieldAttachArgs(this, null, null));
            if (source is BattleFieldCard killer && !killer.IsKilled)
                await killer._onKillConfirmed.Invoke(killer, new BattleKillConfirmArgs(this, args));

            DestroyDrawer(false);
            Dispose();
        }

        public override async UniTask<bool> CanBeAttachedToField(TableFieldAttachArgs e)
        {
            bool result = await base.CanBeAttachedToField(e);
            if (!result) return result;
            return await _onFieldTryToAttach.InvokeAND(this, e);
        }
        public BattleInitiationSendArgs CreateInitiation()
        {
            int strengthPerTarget = (Strength / (float)Range.splash.targetsCount).Ceiling();
            return new BattleInitiationSendArgs(this, strengthPerTarget, topPriority: false, manualAim: Side.isMe);
        }
        public BattleWeight CalculateWeight(params int[] excludedWeights)
        {
            if (_isKilled)
                return BattleWeight.Zero(this);

            float absWeight = ((int)Health).ClampedMin(0) + ((int)Strength).ClampedMin(0) * 2;
            float relWeight = 0;

            foreach (IBattleTraitListElement element in Traits)
            {
                if (excludedWeights.Contains(element.Trait.Guid)) continue;
                BattleWeight weight = element.Trait.CalculateWeight(excludedWeights);
                absWeight += weight.absolute;
                relWeight += weight.relative;
            }

            return new BattleWeight(this, absWeight, relWeight);
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
            if (e.field == null)
            {
                await _onFieldPreAttached.Invoke(this, e);
                await SetObserveTargets(false);
                await base.AttachToFieldInternal(e);
                await _onFieldPostAttached.Invoke(this, e);
                return;
            }

            await _onFieldPreAttached.Invoke(this, e);
            await SetObserveTargets(false);

            _side = ((BattleField)e.field).Side;
            await base.AttachToFieldInternal(e);

            if (IsKilled) return;
            await SetObserveTargets(true);
            await _onFieldPostAttached.Invoke(this, e);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleFieldCardDrawer(this, parent) { SortingOrderDefault = 10 };
        }

        protected override async UniTask OnStatPreSetBase_TOP(object sender, TableStat.PreSetArgs e)
        {
            await base.OnStatPreSetBase_TOP(sender, e);

            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            string cardName = card.TableNameDebug;
            string statName = stat.TableNameDebug;
            string sourceName = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardName}: stats: {statName}: OnPreSet: delta: {e.deltaValue} (by: {sourceName}).");
        }
        protected override async UniTask OnStatPostSetBase_TOP(object sender, TableStat.PostSetArgs e)
        {
            await base.OnStatPostSetBase_TOP(sender, e);

            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            string cardNameDebug = card.TableNameDebug;
            string statNameDebug = stat.TableNameDebug;
            string sourceNameDebug = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardNameDebug}: stats: {statNameDebug}: OnPostSet: delta: {e.totalDeltaValue} (by: {sourceNameDebug}).");
        }

        protected virtual UniTask<bool> OnFieldTryToAttachBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardNameDebug = card?.TableNameDebug;
            string fieldNameDebug = e.field?.TableNameDebug;
            string sourceNameDebug = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardNameDebug}: field: TryToAttach: field: {fieldNameDebug} (by: {sourceNameDebug}).");
            return UniTask.FromResult(true);
        }
        protected virtual UniTask OnFieldPreAttachedBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardNameDebug = card.TableNameDebug;
            string fieldNameDebug = e.field?.TableNameDebug;
            string sourceNameDebug = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardNameDebug}: field: PreAttached: field: {fieldNameDebug} (by: {sourceNameDebug}).");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnFieldPostAttachedBase_TOP(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            BattleTerritory terr = card.Territory;

            string cardNameDebug = card.TableNameDebug;
            string fieldNameDebug = e.field?.TableNameDebug;
            string sourceNameDebug = e.source?.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardNameDebug}: field: PostAttached: field: {fieldNameDebug} (by: {sourceNameDebug}).");
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnPreKilledBase_TOP(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardName = card.TableName;
            string cardNameDebug = card.TableNameDebug;
            string sourceName = e.source?.TableName;
            string sourceNameDebug = e.source?.TableNameDebug;
            TableConsole.LogToFile("card", $"{cardNameDebug}: OnPreKilled (by: {sourceNameDebug}).");
            return UniTask.FromResult(true);
        }
        protected virtual UniTask OnPostKilledBase_TOP(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardName = card.TableName;
            string cardNameDebug = card.TableNameDebug;
            string sourceName = e.source?.TableName;
            string sourceNameDebug = e.source?.TableNameDebug;
            TableConsole.LogToFile("card", $"{cardNameDebug}: OnPostKilled (by: {sourceNameDebug})");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnEvadedBeingKilledBase_TOP(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardName = card.TableNameDebug;
            string cardNameDebug = card.TableNameDebug;
            string sourceName = e.source?.TableName;
            string sourceNameDebug = e.source?.TableNameDebug;
            TableConsole.LogToFile("card", $"{cardNameDebug}: evaded being killed (by: {sourceNameDebug})");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnKillConfirmedBase_TOP(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard card = (BattleFieldCard)sender;
            string cardNameDebug = card.TableNameDebug;
            string victimNameDebug = e.victim.TableNameDebug;

            TableConsole.LogToFile("card", $"{cardNameDebug}: kill confirmed: victim: {victimNameDebug}");
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnInitiationPreSentBase_TOP(object sender, BattleInitiationSendArgs e)
        {
            string senderName = e.Sender.TableNameDebug;
            string receiversNames = null;
            string manualStr = e.manualAim ? "M" : "";
            foreach (BattleField receiver in e.Receivers)
            {
                if (receiversNames == null)
                    receiversNames = receiver.TableNameDebug;
                else receiversNames += $", {receiver.TableNameDebug}";
            }

            TableConsole.LogToFile("card", $"{senderName}: initiation: OnPreSent: strength: {e.Strength}{manualStr}, targets: {receiversNames}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostSentBase_TOP(object sender, BattleInitiationSendArgs e)
        {
            string senderName = e.Sender.TableNameDebug;
            string receiversNames = null;
            string manualStr = e.manualAim ? "M" : "";
            foreach (BattleField receiver in e.Receivers)
            {
                if (receiversNames == null)
                    receiversNames = receiver.TableNameDebug;
                else receiversNames += $", {receiver.TableNameDebug}";
            }

            TableConsole.LogToFile("card", $"{senderName}: initiation: OnPostSent: strength: {e.Strength}{manualStr}, targets: {receiversNames}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationConfirmedBase_TOP(object sender, BattleInitiationRecvArgs e)
        {
            string senderName = e.Sender.TableNameDebug;
            string receiverName = e.ReceiverField.TableNameDebug;

            TableConsole.LogToFile("card", $"{senderName}: initiation: OnConfirmed: strength: {e.Strength}, target: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPreReceivedBase_TOP(object sender, BattleInitiationRecvArgs e)
        {
            string senderName = e.Sender.TableNameDebug;
            string receiverName = e.ReceiverField.TableNameDebug;

            TableConsole.LogToFile("card", $"{senderName}: initiation: OnPreReceived: strength: {e.Strength}, target: {receiverName}.");
            return UniTask.CompletedTask;
        }
        protected virtual UniTask OnInitiationPostReceivedBase_TOP(object sender, BattleInitiationRecvArgs e)
        {
            string senderName = e.Sender.TableNameDebug;
            string receiverName = e.ReceiverField.TableNameDebug;

            TableConsole.LogToFile("card", $"{senderName}: initiation: OnPostReceived: strength: {e.Strength}, target: {receiverName}.");
            return UniTask.CompletedTask;
        }

        async UniTask SetObserveTargets(bool value)
        {
            await _area.SetObserveTargets(value);
            IEnumerable<IBattleTraitListElement> traits = ((IEnumerable<IBattleTraitListElement>)Traits).ToArray();
            foreach (IBattleTraitListElement element in traits)
                await element.Trait.Area.SetObserveTargets(value);
        }
        async UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            if (e.newStatValue > 0) return;
            TableStat stat = (TableStat)sender;
            BattleFieldCard card = (BattleFieldCard)stat.Owner;
            await card.TryKill(BattleKillMode.Default, e.source);
        }
        async UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card.Field == null || card._isKilled) return;
            if (++card._phaseAge <= MOXIE_PRIORITY_VALUE) return;

            card.Drawer.CreateTextAsSpeech("* СТАРОСТЬ *", Color.red);
            await card.TryKill(BattleKillMode.IgnoreEverything, null);
        }
        async UniTask OnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattleFieldCard card = (BattleFieldCard)Finder.FindInBattle(territory);

            if (card == null) return;
            if (card.Field == null || card._isKilled)
                return;

            card.TurnAge++;
            foreach (ITableTraitListElement element in card.Traits)
            {
                ITableTrait trait = element.Trait;
                trait.TurnDelay--;
                trait.TurnAge++;
            }
        }

        int CalculateInitiationOrder()
        {
            List<BattleFieldCard> cards = Side.Territory.Fields().WithCard().Select(f => f.Card).ToList();
            cards.Sort((x, y) => -x.InitiationPriority.CompareTo(y.InitiationPriority)); // reversed
            return cards.IndexOf(this) + 1;
        }
    }
}
