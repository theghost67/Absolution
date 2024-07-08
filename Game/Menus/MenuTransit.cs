using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

namespace Game.Menus
{
    /// <summary>
    /// Класс, представляющий промежуточное меню между двумя другими меню. Реализует простую анимацию перехода между двумя типами <see cref="Menu"/>.
    /// </summary>
    public class MenuTransit : Menu
    {
        const string ID = "transit";

        const float DURATION = 0.75f;
        const float Y_TO_OPENED = 0f;
        const float Y_TO_CLOSED = 3.6f;
        const float Y_TO_RESET = -3.6f;

        static readonly GameObject _prefab = Resources.Load<GameObject>($"Prefabs/Menus/{ID}");
        static readonly MenuTransit _instance = new();

        Tween _tween;
        private MenuTransit() : base(ID, _prefab) { }

        // call to?.SetColliders(true) in TransitToThis
        // call from?.SetColliders(false) in TransitFromThis
        public static UniTask Between(Menu? from, Menu? to, Action? action = null)
        {
            return _instance.BetweenInternal(from, to, action);
        }
        async UniTask BetweenInternal(Menu? from, Menu? to, Action? action)
        {
            from?.OnTransitStart(true);
            to?.OnTransitStart(false);
            await AnimShow();
            await UniTask.Delay((int)(DURATION * 1000));
            from?.Close();
            to?.Open();
            from?.OnTransitMiddle(true);
            to?.OnTransitMiddle(false);
            action?.Invoke();
            await AnimHide();
            from?.OnTransitEnd(true);
            to?.OnTransitEnd(false);
        }

        public UniTask AnimShow()
        {
            Open();

            _tween.Kill();
            _tween = Transform.DOLocalMoveY(Y_TO_OPENED, DURATION).SetEase(Ease.InOutQuad);
            return _tween.AsyncWaitForCompletion();
        }
        public UniTask AnimHide()
        {
            _tween.Kill();
            _tween = Transform.DOLocalMoveY(Y_TO_CLOSED, DURATION).SetEase(Ease.InOutQuad).OnComplete(OnCloseTweenComplete);
            return _tween.AsyncWaitForCompletion();
        }

        void OnCloseTweenComplete()
        {
            Transform.localPosition = Vector3.up * Y_TO_RESET;
            Close();
        }
    }
}
