using Cysharp.Threading.Tasks;
using DG.Tweening;
using GreenOne;
using System;
using Unity.VisualScripting;
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

        public static UniTask Between(Menu? from, Menu? to, Action? action = null)
        {
            return _instance.BetweenInternal(from, to, action, false);
        }
        public static UniTask BetweenWithColliders(Menu? from, Menu? to, Action? action = null)
        {
            return _instance.BetweenInternal(from, to, null, true);
        }
        async UniTask BetweenInternal(Menu from, Menu to, Action action, bool setColliders)
        {
            from?.SetColliders(false);
            await OpenAnimated();
            await UniTask.Delay((int)(DURATION * 1000));
            from?.CloseInstantly();
            to?.OpenInstantly();
            if (setColliders)
                to?.SetColliders(false);
            action?.Invoke();
            await CloseAnimated();
            if (setColliders)
                to?.SetColliders(true);
        }

        public override UniTask OpenAnimated()
        {
            OpenInstantly();

            _tween.Kill();
            _tween = Transform.DOLocalMoveY(Y_TO_OPENED, DURATION).SetEase(Ease.InOutQuad);
            return _tween.AsyncWaitForCompletion();
        }
        public override UniTask CloseAnimated()
        {
            _tween.Kill();
            _tween = Transform.DOLocalMoveY(Y_TO_CLOSED, DURATION).SetEase(Ease.InOutQuad).OnComplete(OnCloseTweenComplete);
            return _tween.AsyncWaitForCompletion();
        }

        void OnCloseTweenComplete()
        {
            Transform.localPosition = Vector3.up * Y_TO_RESET;
            CloseInstantly();
        }
    }
}
