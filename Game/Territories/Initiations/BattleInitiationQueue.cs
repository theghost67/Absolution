﻿using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.Territories
{
    // MAIN NOTES:
    // 1. Positive strength = damage, negative strength = healing.
    // 2. Initiations are ordered by 'Priority'. Initiations with higher priority dequeue first.
    // 3. If initiation has multiple targets, it will process one target at a time, sorting them by their 'Priority' ascending.
    // 4. BattleInitiationRecvArgs.Receiver can be changed to redirect the initiation to other receiver.

    /// <summary>
    /// Представляет очередь инициаций карт на территории сражения (см. <see cref="BattleTerritory"/>).
    /// </summary>
    public sealed class BattleInitiationQueue : IDisposable
    {
        public static bool IsAnyRunning => _isAnyRunning;
        public bool IsRunning => _isRunning;
        public BattleTerritory Territory => _territory;
        public int Count => _list.Count;
        public float SpeedScale => _speedScale; // TODO: implement 'set' (set time scale for each active tween)

        public event EventHandler OnStarted;
        public event EventHandler OnEnded;
        public event EventHandler OnceComplete;

        static readonly GameObject _initiationPreviewPrefab;
        static readonly GameObject _initiationInMovePrefab;
        static readonly GameObject _initiationBlankPrefab;

        static readonly Sprite _initiationPreviewSenderDmgSprite;
        static readonly Sprite _initiationPreviewSenderHealSprite;
        static readonly Sprite _initiationPreviewReceiverDmgSprite;
        static readonly Sprite _initiationPreviewReceiverHealSprite;

        static bool _isAnyRunning;
        readonly BattleTerritory _territory;
        readonly List<BattleInitiationSendArgs> _list;

        Tween _iShowTween;
        Tween _iUpdateTween;
        Tween _iRedirectTween;
        Tween _iHideTween;
        float _speedScale;
        bool _isRunning;

        static BattleInitiationQueue()
        {
            _initiationPreviewPrefab = Resources.Load<GameObject>("Prefabs/Territories/Initiation preview");
            _initiationInMovePrefab = Resources.Load<GameObject>("Prefabs/Territories/Initiation in move");
            _initiationBlankPrefab = Resources.Load<GameObject>("Prefabs/Territories/Initiation blank");

            _initiationPreviewSenderDmgSprite = Resources.Load<Sprite>("Sprites/Territories/initiation preview bg sender damaging");
            _initiationPreviewSenderHealSprite = Resources.Load<Sprite>("Sprites/Territories/initiation preview bg sender healing");
            _initiationPreviewReceiverDmgSprite = Resources.Load<Sprite>("Sprites/Territories/initiation preview bg receiver damaging");
            _initiationPreviewReceiverHealSprite = Resources.Load<Sprite>("Sprites/Territories/initiation preview bg receiver healing");
        }
        public BattleInitiationQueue(BattleTerritory territory)
        {
            _territory = territory;
            _list = new List<BattleInitiationSendArgs>(BattleTerritory.MAX_SIZE);
            _speedScale = 1;
        }

        public void Dispose()
        {
            _list.Clear();
        }
        public void Run()
        {
            if (!_isRunning && _list.Count != 0)
                QueueLoop();
        }

        public void EnqueueAndRun(params BattleInitiationSendArgs[] initiations)
        {
            EnqueueAndRun((IEnumerable<BattleInitiationSendArgs>)initiations);
        }
        public void EnqueueAndRun(IEnumerable<BattleInitiationSendArgs> initiations)
        {
            foreach (BattleInitiationSendArgs sArgs in initiations)
                _list.Add(sArgs);
            Run();
        }

        public void Enqueue(params BattleInitiationSendArgs[] initiations)
        {
            Enqueue((IEnumerable<BattleInitiationSendArgs>)initiations);
        }
        public void Enqueue(IEnumerable<BattleInitiationSendArgs> initiations)
        {
            foreach (BattleInitiationSendArgs sArgs in initiations)
                _list.Add(sArgs);
        }

        public async UniTask Await()
        {
            while (_isRunning)
                await UniTask.Yield();
        }
        async UniTask QueueLoop()
        {
            const int KILLED = -1;
            const int HANDLED = 0;
            const int NOT_HANDLED = 1;

            _isRunning = true;
            OnStarted?.Invoke(this, EventArgs.Empty);

            while (_list.Count != 0)
            {
                BattleInitiationSendArgs sArgs = _list.Max();
                _list.Remove(sArgs);

                BattleFieldCard sender = sArgs.Sender;
                int TryHandleInitiation(bool countReceivers)
                {
                    if (sArgs.Sender.IsKilled) return KILLED;
                    if (countReceivers && sArgs.Receivers.Count == 0)
                        return HANDLED;
                    if (sArgs.handled || !sender.CanInitiate || sender.strength <= 0)
                        return HANDLED;
                    return NOT_HANDLED;
                }

                switch (TryHandleInitiation(false))
                {
                    case KILLED: await HideInitiationPreviews(sArgs); continue;
                    case HANDLED: await AnimInitiationBlank(sArgs.Sender); continue;
                    case NOT_HANDLED: break;
                    default: throw new NotImplementedException();
                }

                await ShowInitiationPreview(sArgs.Sender, sArgs.strength, fieldIsSender: true).AsyncWaitForCompletion();
                sArgs_InitDrawableEvents(sArgs);

                await sArgs.SelectReceivers();
                await InvokeSendArgsEvent(sender.OnInitiationPreSent, sender, sArgs);
                switch (TryHandleInitiation(true))
                {
                    case KILLED: continue;
                    case HANDLED: await AnimInitiationBlank(sArgs.Sender); continue;
                    case NOT_HANDLED: break;
                    default: throw new NotImplementedException();
                }

                await ShowInitiationPreviews(sArgs);
                while (sArgs.Receivers.Count != 0)
                {
                    BattleField receiver = sArgs.ReceiversByPriority.First();
                    if (TryHandleInitiation(true) == NOT_HANDLED)
                        await AnimInitiationToField(receiver, sArgs);
                    else break;
                    sArgs.RemoveReceiver(receiver);
                }
                if (!sArgs.Sender.IsKilled)
                    await InvokeSendArgsEvent(sender.OnInitiationPostSent, sender, sArgs);
                await HideInitiationPreviews(sArgs);
            }

            if (!_territory.DrawersAreNull)
                await UniTask.Delay((int)(ANIM_DURATION_HALF * 1000));

            _isRunning = false;
            OnEnded?.Invoke(this, EventArgs.Empty);

            OnceComplete?.Invoke(this, EventArgs.Empty);
            OnceComplete = null;
        }

        #region animations
        const float ANIM_DURATION = 1.0f;
        const float ANIM_DURATION_HALF = ANIM_DURATION / 2;

        const string SENDER_TAG = "Initiation sender";
        const string RECEIVER_TAG = "Initiation receiver";

        // also invokes initiation receive
        async UniTask AnimInitiationToField(BattleField field, BattleInitiationSendArgs sArgs)
        {
            if (field.Card != null)
            {
                await AnimInitiationToCard(field.Card, sArgs);
                return;
            }

            BattleInitiationRecvArgs rArgs = new(field, sArgs);
            rArgs_InitDrawableEvents(rArgs);
            // can invoke OnInitiationPreReceived events here (if BattleField ever gets one)

            await AnimInitiationMove(sArgs, rArgs);
            HideInitiationPreview(rArgs.Receiver, instantly: true);
            OnInitiationReceivedByField(field, rArgs);

            BattleFieldCard sender = sArgs.Sender;
            await InvokeRecvArgsEvent(sender.OnInitiationConfirmed, sender, rArgs);
        }
        async UniTask AnimInitiationToCard(BattleFieldCard card, BattleInitiationSendArgs sArgs)
        {
            if (card.IsKilled) return;

            BattleInitiationRecvArgs rArgs = new(card.Field, sArgs);
            rArgs_InitDrawableEvents(rArgs);
            await InvokeRecvArgsEvent(card.OnInitiationPreReceived, card, rArgs);

            if (rArgs.handled)
            {
                await AnimInitiationBlank(card.Field);
                return;
            }
            if (rArgs.Receiver.Card != card) // initiation can be redirected in OnInitiationPreReceived method
            {
                await AnimInitiationToField(rArgs.Receiver, sArgs); 
                return;
            }

            await AnimInitiationMove(sArgs, rArgs);
            HideInitiationPreview(rArgs.Receiver, instantly: true);
            OnInitiationReceivedByCard(card, rArgs);

            BattleFieldCard sender = sArgs.Sender;
            await InvokeRecvArgsEvent(sArgs.Sender.OnInitiationConfirmed, sender, rArgs);
            await InvokeRecvArgsEvent(card.OnInitiationPostReceived, card, rArgs);
        }

        UniTask ShowInitiationPreviews(BattleInitiationSendArgs sArgs)
        {
            if (sArgs.Sender.Drawer == null)
                return UniTask.CompletedTask;

            sArgs.Sender.OnPostKilled.Add("init_preview", (s, e) => HideInitiationPreviews(sArgs));

            Tween lastTargetTween = null;
            foreach (BattleField target in sArgs.Receivers)
                lastTargetTween = ShowInitiationPreview(target, sArgs.strength, fieldIsSender: false);

            Tween senderTween = ShowInitiationPreview(sArgs.Sender, sArgs.strength, fieldIsSender: true);
            if (senderTween != null && !senderTween.IsComplete())
                 return senderTween.AsyncWaitForCompletion();
            else return lastTargetTween.AsyncWaitForCompletion();
        }
        UniTask HideInitiationPreviews(BattleInitiationSendArgs sArgs) 
        {
            if (sArgs.Sender.Drawer == null)
                return UniTask.CompletedTask;

            sArgs.Sender.OnPostKilled.Remove("init_preview");

            // preview is hidden once received by target or when the initiation is handled 
            return HideInitiationPreview(sArgs.Sender, instantly: false).AsyncWaitForCompletion();
        }

        UniTask AnimInitiationMove(BattleInitiationSendArgs sArgs, BattleInitiationRecvArgs rArgs)
        {
            if (sArgs.Sender.Drawer == null)
                return UniTask.CompletedTask;

            Vector3 from = sArgs.Sender.Drawer.transform.position;
            Vector3 to = rArgs.Receiver.Drawer.transform.position;

            GameObject iInMovePrefab = GameObject.Instantiate(_initiationInMovePrefab, Global.Root);
            Transform iInMoveTransform = iInMovePrefab.transform;
            TextMeshPro iInMoveText = iInMoveTransform.Find<TextMeshPro>("Text");
            int strength = sArgs.strength; 

            iInMoveTransform.position = from;
            iInMoveText.text = strength.Abs().ToString();
            iInMoveText.color = strength < 0 ? Color.green : Color.red;

            Tween tween = iInMoveText.transform.DOMove(to, ANIM_DURATION_HALF).SetEase(Ease.InExpo).OnComplete(iInMovePrefab.Destroy);
            return tween.AsyncWaitForCompletion();
        }
        UniTask AnimInitiationBlank(ITableObject obj)
        {
            if (obj.Drawer == null)
                return UniTask.CompletedTask;

            HideInitiationPreview(obj, instantly: true);

            GameObject iBlankPrefab = GameObject.Instantiate(_initiationBlankPrefab, obj.Drawer.transform);
            Transform iBlankTransform = iBlankPrefab.transform;
            SpriteRenderer iBlankBg = iBlankPrefab.Find<SpriteRenderer>("Bg");

            iBlankTransform.localScale = Vector3.one * 1.35f;
            iBlankBg.DOColor(iBlankBg.color.WithAlpha(0), ANIM_DURATION).SetEase(Ease.InQuad);

            Tween tween = iBlankTransform.DOScale(Vector3.one, ANIM_DURATION).SetEase(Ease.OutCubic).OnComplete(iBlankPrefab.Destroy);
            return tween.AsyncWaitForCompletion();
        }

        Tween ShowInitiationPreview(ITableObject obj, int strength, bool fieldIsSender) 
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (obj.Drawer == null)
                return null;
            if (obj.Drawer.transform.Find("Initiation") != null)
                return null;

            BattleFieldCardDrawer cardDrawer;
            if (obj is BattleFieldCard objAsCard)
                 cardDrawer = objAsCard.Drawer;
            else cardDrawer = ((BattleField)obj).Card?.Drawer;
            if (cardDrawer != null)
            {
                TableTraitListSetDrawerElementsCollection elements = cardDrawer.Traits?.elements;
                if (!(cardDrawer.IsSelected && !elements.IsEmpty) || (!elements?.IsRunning ?? true))
                    cardDrawer.ShowBg();
            }

            GameObject previewPrefab = GameObject.Instantiate(_initiationPreviewPrefab, obj.Drawer.transform);
            Transform previewTransform = previewPrefab.transform;
            TextMeshPro previewText = previewPrefab.Find<TextMeshPro>("Text");
            SpriteRenderer previewBg = previewPrefab.Find<SpriteRenderer>("Bg");

            previewPrefab.name = "Initiation";
            previewPrefab.tag = fieldIsSender ? SENDER_TAG : RECEIVER_TAG;
            previewText.color = strength < 0 ? Color.green.WithAlpha(0) : Color.red.WithAlpha(0);
            previewBg.color = Utils.clearWhite;

            if (fieldIsSender)
                 previewBg.sprite = strength < 0 ? _initiationPreviewSenderHealSprite : _initiationPreviewSenderDmgSprite;
            else previewBg.sprite = strength < 0 ? _initiationPreviewReceiverHealSprite : _initiationPreviewReceiverDmgSprite;

            DOVirtual.Float(0, 1, DURATION, v => SetPreviewTextAlpha(previewText, v)).SetEase(Ease.OutCubic);
            DOVirtual.Float(0, 1, DURATION, v => SetPreviewBgPosAndColor(previewBg, v)).SetEase(Ease.OutCubic);

            previewText.transform.localScale = Vector3.one * 0.5f;
            previewText.transform.DOScale(Vector3.one, DURATION).SetEase(Ease.OutCubic);

            _iShowTween = DOVirtual.Int(0, strength, DURATION, v => SetPreviewStrength(previewText, previewBg, v)).SetEase(Ease.OutCubic);
            return _iShowTween;
        }
        Tween UpdateInitiationPreview(ITableObject obj, int strength)
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (obj.Drawer == null)
                return null;

            Transform previewTransform = obj.Drawer.transform.Find("Initiation");
            if (previewTransform == null) return null;

            TextMeshPro previewText = previewTransform.Find<TextMeshPro>("Text");
            SpriteRenderer previewBg = previewTransform.Find<SpriteRenderer>("Bg");
            int prevValue = int.Parse(previewText.text);

            int ID = 0x00BA771E + obj.Guid;
            DOTween.Kill(ID);

            _iUpdateTween = DOVirtual.Int(prevValue, strength, DURATION, v => SetPreviewStrength(previewText, previewBg, v)).SetId(ID);
            return _iUpdateTween;
        }
        Tween RedirectInitiationPreview(ITableObject objOld, ITableObject objNew)
        {
            if (objNew.Drawer == null)
                return null;

            Transform oldPreviewTransform = objOld.Drawer.transform.Find("Initiation");
            if (oldPreviewTransform == null) return null;
            oldPreviewTransform.SetParent(objNew.Drawer.transform, worldPositionStays: true);

            //TextMeshPro previewText = oldPreviewTransform.Find<TextMeshPro>("Text");
            //previewText.color = Color.white.WithAlpha(previewText.color.a);

            _iRedirectTween = oldPreviewTransform.DOMove(objNew.Drawer.transform.position, 0.5f).SetEase(Ease.OutExpo);
            return _iRedirectTween;
        }
        Tween HideInitiationPreview(ITableObject obj, bool instantly)
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (obj.Drawer == null)
                return null;

            Transform previewTransform = obj.Drawer.transform.Find("Initiation");
            if (previewTransform == null) return null;
            if (instantly)
            {
                previewTransform.gameObject.Destroy();
                return null;
            }

            BattleFieldCardDrawer cardDrawer;
            if (obj is BattleFieldCard objAsCard)
                cardDrawer = objAsCard.Drawer;
            else cardDrawer = ((BattleField)obj).Card?.Drawer;
            if (cardDrawer != null)
            {
                TableTraitListSetDrawerElementsCollection elements = cardDrawer.Traits?.elements;
                if (!(cardDrawer.IsSelected && !elements.IsEmpty) || (!elements?.IsRunning ?? true))
                    cardDrawer.HideBg();
            }

            previewTransform.gameObject.name = "Initiation (hiding)";
            TextMeshPro previewText = previewTransform.Find<TextMeshPro>("Text");
            SpriteRenderer previewBg = previewTransform.Find<SpriteRenderer>("Bg");

            DOVirtual.Float(1, 0, DURATION, v => SetPreviewTextAlpha(previewText, v)).SetEase(Ease.InCubic);
            DOVirtual.Float(1, 0, DURATION, v => SetPreviewBgPosAndColor(previewBg, v)).SetEase(Ease.InCubic);

            _iHideTween = previewText.transform.DOScale(Vector3.one * 0.5f, DURATION).SetEase(Ease.InCubic).OnComplete(previewTransform.gameObject.Destroy);
            return _iHideTween;
        }

        static void SetPreviewBgPosAndColor(SpriteRenderer previewBg, float value)
        {
            const float MAX_POS_Y = 0.0f;
            const float MIN_POS_Y = -0.1f;

            const float MAX_ALPHA = 0.3f;
            const float MIN_ALPHA = 0.0f;

            previewBg.transform.localPosition = Vector3.up * Mathf.Lerp(MIN_POS_Y, MAX_POS_Y, value);
            previewBg.color = Color.white.WithAlpha(Mathf.Lerp(MIN_ALPHA, MAX_ALPHA, value));
        }
        static void SetPreviewTextAlpha(TextMeshPro previewText, float value)
        {
            const float MAX_ALPHA = 0.5f;
            const float MIN_ALPHA = 0.0f;

            previewText.color = previewText.color.WithAlpha(Mathf.Lerp(MIN_ALPHA, MAX_ALPHA, value));
        }

        static int GetPreviewStrength(BattleField field)
        {
            Transform previewTransform = field.Drawer?.transform.Find("Initiation");
            if (previewTransform == null) return 0;
            TextMeshPro previewText = previewTransform.Find<TextMeshPro>("Text");
            return Convert.ToInt32(previewText.text);
        }
        static void SetPreviewStrength(TextMeshPro previewText, SpriteRenderer previewBg, int strength)
        {
            int prevValue = int.Parse(previewText.text);
            if (prevValue == strength) return;

            previewText.text = strength.Abs().ToString();
            bool fieldIsSender = previewText.transform.parent.CompareTag(SENDER_TAG);
            if (fieldIsSender)
                 previewBg.sprite = strength < 0 ? _initiationPreviewSenderHealSprite : _initiationPreviewSenderDmgSprite;
            else previewBg.sprite = strength < 0 ? _initiationPreviewReceiverHealSprite : _initiationPreviewReceiverDmgSprite;

            if (strength < 0)
                 previewText.color = Color.green.WithAlpha(previewText.color.a);
            else previewText.color = Color.red.WithAlpha(previewText.color.a);
        }
        #endregion

        #region events
        void sArgs_InitDrawableEvents(BattleInitiationSendArgs sArgs)
        {
            sArgs.OnReceiverAdded += sArgs_OnInitiationReceiverAdded;
            sArgs.OnReceiverRemoved += sArgs_OnInitiationReceiverRemoved;
            sArgs.OnStrengthChanged += sArgs_OnInitiationStrengthChanged;
        }
        void sArgs_OnInitiationReceiverAdded(object sender, BattleField receiver)
        {
            BattleInitiationSendArgs sArgs = (BattleInitiationSendArgs)sender;
            ShowInitiationPreview(receiver, sArgs.strength, false);
        }
        void sArgs_OnInitiationReceiverRemoved(object sender, BattleField receiver)
        {
            //BattleInitiationSendArgs sArgs = (BattleInitiationSendArgs)sender;
            HideInitiationPreview(receiver, false);
        }
        void sArgs_OnInitiationStrengthChanged(object sender, int strength)
        {
            BattleInitiationSendArgs sArgs = (BattleInitiationSendArgs)sender;
            foreach (BattleField receiver in sArgs.Receivers)
                UpdateInitiationPreview(receiver, strength);
        }

        // TODO: create arrow object that points to a new receiver
        void rArgs_InitDrawableEvents(BattleInitiationRecvArgs rArgs)
        {
            rArgs.OnReceiverChanged += rArgs_OnInitiationReceiverChanged;
            rArgs.OnStrengthChanged += rArgs_OnInitiationStrengthChanged;
        }
        void rArgs_OnInitiationReceiverChanged(object sender, BattleField receiver)
        {
            BattleInitiationRecvArgs rArgs = (BattleInitiationRecvArgs)sender;
            RedirectInitiationPreview(rArgs.receiverDefault, rArgs.Receiver);
        }
        void rArgs_OnInitiationStrengthChanged(object sender, int strength)
        {
            BattleInitiationRecvArgs rArgs = (BattleInitiationRecvArgs)sender;
            UpdateInitiationPreview(rArgs.receiverDefault, strength);
        }
        #endregion

        #region IBattleEntities events
        // probably not the best solution, but it is clean

        void OnInitiationReceivedByField(BattleField field, BattleInitiationRecvArgs rArgs)
        {
            Drawer drawer = field.Drawer;
            if (drawer != null)
            {
                drawer.transform.DOAShake();
                drawer.CreateTextAsDamage(((float)rArgs.strength).Abs().Ceiling(), rArgs.strength < 0);
            }
            field.health.AdjustValue(-rArgs.strength, rArgs.Sender);
        }
        void OnInitiationReceivedByCard(BattleFieldCard card, BattleInitiationRecvArgs rArgs)
        {
            Drawer drawer = card.Drawer;
            if (drawer != null)
            {
                drawer.transform.DOAShake();
                drawer.CreateTextAsDamage(((float)rArgs.strength).Abs().Ceiling(), rArgs.strength < 0);
            }
            card.health.AdjustValue(-rArgs.strength, rArgs.Sender);
        }

        UniTask InvokeSendArgsEvent(IIdEventVoidAsync<BattleInitiationSendArgs> @event, object sender, BattleInitiationSendArgs e)
        {
            ((TableEventVoid<BattleInitiationSendArgs>)@event).Invoke(sender, e);
            if (e.Sender.Drawer != null)
                 return AwaitTweensAndEvents();
            else return UniTask.CompletedTask;
        }
        UniTask InvokeRecvArgsEvent(IIdEventVoidAsync<BattleInitiationRecvArgs> @event, object sender, BattleInitiationRecvArgs e)
        {
            ((TableEventVoid<BattleInitiationRecvArgs>)@event).Invoke(sender, e);
            if (e.Receiver.Drawer != null)
                return AwaitTweensAndEvents();
            else return UniTask.CompletedTask;
        }

        async UniTask AwaitTweensAndEvents()
        {
            while (TableEventManager.CanAwaitTableEvents()  ||
                   TweenIsRunning(_iShowTween)              ||
                   TweenIsRunning(_iUpdateTween)            || 
                   TweenIsRunning(_iRedirectTween)          || 
                   TweenIsRunning(_iHideTween))
                await UniTask.Yield();
        }
        static bool TweenIsRunning(Tween t)
        {
            return t.IsActive() && !t.IsComplete();
        }
        #endregion
    }
}
