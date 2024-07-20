using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Game.Territories
{
    // MAIN NOTES:
    // 1. Positive strength = damage, negative strength = healing.
    // 2. Initiations are ordered by 'Priority'. Initiations with higher priority dequeue first.
    // 3. If initiation has multiple targets, it will process one target at a time, sorting them by their 'Priority' ascending.
    // 4. BattleInitiationRecvArgs.Receiver can be changed to redirect the initiation to other receiver (it will work ONLY ONCE).

    /// <summary>
    /// Представляет очередь инициаций карт на территории сражения (см. <see cref="BattleTerritory"/>).
    /// </summary>
    public sealed class BattleInitiationQueue : IDisposable
    {
        public event EventHandler OnStarted;
        public event EventHandler OnEnded;
        public event EventHandler OnceComplete;

        public BattleTerritory Territory => _territory;
        public float SpeedScale => _speedScale; // TODO: implement 'set' (set time scale for each active tween)
        public int Count => _list.Count;

        static readonly GameObject _initiationPreviewPrefab;
        static readonly GameObject _initiationInMovePrefab;
        static readonly GameObject _initiationBlankPrefab;

        static readonly Sprite _initiationPreviewSenderDmgSprite;
        static readonly Sprite _initiationPreviewSenderHealSprite;
        static readonly Sprite _initiationPreviewReceiverDmgSprite;
        static readonly Sprite _initiationPreviewReceiverHealSprite;

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
            _isRunning = true;
            OnStarted?.Invoke(this, EventArgs.Empty);

            while (_list.Count != 0)
            {
                BattleInitiationSendArgs sArgs = _list.Max();
                _list.Remove(sArgs);

                BattleFieldCard sender = sArgs.Sender;
                if (sender.IsKilled) continue;
                if (sArgs.handled || !sender.CanInitiate || sender.strength <= 0)
                {
                    await AnimInitiationBlank(sArgs.Sender.Field);
                    continue;
                }

                await ShowInitiationPreview(sArgs.Sender.Field, sArgs.strength, fieldIsSender: true).AsyncWaitForCompletion();
                sArgs_InitDrawableEvents(sArgs);

                await sArgs.SelectReceivers();
                await InvokeSendArgsEvent(sender.OnInitiationPreSent, sender, sArgs);
                if (sArgs.handled || sArgs.Receivers.Count == 0)
                {
                    await AnimInitiationBlank(sArgs.Sender.Field);
                    continue;
                }

                await ShowInitiationPreviews(sArgs);
                bool receiversSkipped = false;
                foreach (BattleField receiver in sArgs.ReceiversByPriority)
                {
                    if (!sender.IsKilled && sender.CanInitiate)
                        await AnimInitiationToField(receiver, sArgs);
                    else
                    {
                        receiversSkipped = true;
                        break;
                    }
                }

                if (!receiversSkipped)
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
            if (sArgs.Sender.Field.Drawer == null)
                return UniTask.CompletedTask;

            Tween lastTargetTween = null;
            foreach (BattleField target in sArgs.Receivers)
                lastTargetTween = ShowInitiationPreview(target, sArgs.strength, fieldIsSender: false);

            Tween senderTween = ShowInitiationPreview(sArgs.Sender.Field, sArgs.strength, fieldIsSender: true);
            if (senderTween != null && !senderTween.IsComplete())
                 return senderTween.AsyncWaitForCompletion();
            else return lastTargetTween.AsyncWaitForCompletion();
        }
        UniTask HideInitiationPreviews(BattleInitiationSendArgs sArgs) 
        {
            if (sArgs.Sender.Field.Drawer == null)
                return UniTask.CompletedTask;

            // preview is hidden once received by target 

            //foreach (BattleField target in sArgs.Receivers)
            //    HideInitiationPreview(target, instantly: false);

            return HideInitiationPreview(sArgs.Sender.Field, instantly: false).AsyncWaitForCompletion();
        }

        UniTask AnimInitiationBlank(BattleField field)
        {
            if (field.Drawer == null)
                return UniTask.CompletedTask;

            HideInitiationPreview(field, instantly: true);

            GameObject iBlankPrefab = GameObject.Instantiate(_initiationBlankPrefab, field.Drawer.transform);
            Transform iBlankTransform = iBlankPrefab.transform;
            SpriteRenderer iBlankBg = iBlankPrefab.Find<SpriteRenderer>("Bg");

            iBlankTransform.localScale = Vector3.one * 1.35f;
            iBlankBg.DOColor(iBlankBg.color.WithAlpha(0), ANIM_DURATION).SetEase(Ease.InQuad);

            Tween tween = iBlankTransform.DOScale(Vector3.one, ANIM_DURATION).SetEase(Ease.OutCubic).OnComplete(iBlankPrefab.Destroy);
            return tween.AsyncWaitForCompletion();
        }
        UniTask AnimInitiationMove(BattleInitiationSendArgs sArgs, BattleInitiationRecvArgs rArgs)
        {
            if (sArgs.Sender.Field.Drawer == null)
                return UniTask.CompletedTask;

            Vector3 from = sArgs.Sender.Drawer.transform.position;
            Vector3 to = rArgs.Receiver.Drawer.transform.position;

            GameObject iInMovePrefab = GameObject.Instantiate(_initiationInMovePrefab, sArgs.Sender.Field.Drawer.transform);
            Transform iInMoveTransform = iInMovePrefab.transform;
            TextMeshPro iInMoveText = iInMoveTransform.Find<TextMeshPro>("Text");
            int strength = sArgs.strength; 

            iInMoveTransform.position = from;
            iInMoveText.text = strength.Abs().ToString();
            iInMoveText.color = strength < 0 ? Color.green : Color.red;

            Tween tween = iInMoveText.transform.DOMove(to, ANIM_DURATION_HALF).SetEase(Ease.InExpo).OnComplete(iInMovePrefab.Destroy);
            return tween.AsyncWaitForCompletion();
        }

        Tween ShowInitiationPreview(BattleField field, int strength, bool fieldIsSender) 
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (field.Drawer == null)
                return null;

            if (field.Drawer.transform.Find("Initiation") != null)
                return null;

            if (field.Card != null)
            {
                BattleFieldCardDrawer cardDrawer = field.Card.Drawer;
                if (cardDrawer != null && !cardDrawer.IsSelected)
                    cardDrawer.ShowBg();
            }

            GameObject previewPrefab = GameObject.Instantiate(_initiationPreviewPrefab, field.Drawer.transform);
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
        Tween UpdateInitiationPreview(BattleField field, int strength)
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (field.Drawer == null)
                return null;

            Transform previewTransform = field.Drawer.transform.Find("Initiation");
            if (previewTransform == null) return null;

            TextMeshPro previewText = previewTransform.Find<TextMeshPro>("Text");
            SpriteRenderer previewBg = previewTransform.Find<SpriteRenderer>("Bg");
            int prevValue = int.Parse(previewText.text);

            int ID = 0x00BA771E + field.Guid;
            DOTween.Kill(ID);

            _iUpdateTween = DOVirtual.Int(prevValue, strength, DURATION, v => SetPreviewStrength(previewText, previewBg, v)).SetId(ID);
            return _iUpdateTween;
        }
        Tween RedirectInitiationPreview(BattleField fieldOld, BattleField fieldNew)
        {
            if (fieldNew.Drawer == null)
                return null;

            Transform oldPreviewTransform = fieldOld.Drawer.transform.Find("Initiation");
            if (oldPreviewTransform == null) return null;
            oldPreviewTransform.SetParent(fieldNew.Drawer.transform, worldPositionStays: true);

            //TextMeshPro previewText = oldPreviewTransform.Find<TextMeshPro>("Text");
            //previewText.color = Color.white.WithAlpha(previewText.color.a);

            _iRedirectTween = oldPreviewTransform.DOMove(fieldNew.Drawer.transform.position, 0.5f).SetEase(Ease.OutExpo);
            return _iRedirectTween;
        }
        Tween HideInitiationPreview(BattleField field, bool instantly)
        {
            const float DURATION = ANIM_DURATION_HALF;

            if (field.Drawer == null)
                return null;

            Transform previewTransform = field.Drawer.transform.Find("Initiation");
            if (previewTransform == null) return null;
            if (field.Card != null)
            {
                BattleFieldCardDrawer cardDrawer = field.Card.Drawer;
                if (cardDrawer != null && !cardDrawer.IsSelected)
                    cardDrawer.HideBg();
            }

            if (instantly)
            {
                previewTransform.gameObject.Destroy();
                return null;
            }

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

        UniTask InvokeSendArgsEvent(IIdEventVoidAsync<BattleInitiationSendArgs> @event, object sender, BattleInitiationSendArgs sArgs)
        {
            ((TableEventVoid<BattleInitiationSendArgs>)@event).Invoke(sender, sArgs);
            if (sArgs.Sender.Drawer != null)
                 return AwaitTweensAndEvents();
            else return UniTask.CompletedTask;
        }
        UniTask InvokeRecvArgsEvent(IIdEventVoidAsync<BattleInitiationRecvArgs> @event, object sender, BattleInitiationRecvArgs rArgs)
        {
            ((TableEventVoid<BattleInitiationRecvArgs>)@event).Invoke(sender, rArgs);
            if (rArgs.Receiver.Drawer != null)
                return AwaitTweensAndEvents();
            else return UniTask.CompletedTask;
        }

        async UniTask AwaitTweensAndEvents()
        {
            while (TableEventManager.IsAnyRunning  ||
                   TweenIsRunning(_iShowTween)     ||
                   TweenIsRunning(_iUpdateTween)   || 
                   TweenIsRunning(_iRedirectTween) || 
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
