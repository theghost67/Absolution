using DG.Tweening;
using DG.Tweening.Core.Enums;
using DG.Tweening.Core;
using GreenOne;
using MyBox;
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Effects
{
    // TODO: consider rewriting (especially DOAExplosion (float card use anim))
    public static class Anim
    {
        public static Tween DOAShake(this Transform transform, float power = 0.02f, float duration = 0.25f)
        {
            if (transform == null) return Utils.emptyTween;
            transform.DOKill(complete: true);
            Vector3 defaultPos = transform.localPosition;

            Tween tween = transform.DOShakePosition2D(duration, power, 50).SetTarget(transform);
            return tween.OnComplete(() => transform.localPosition = defaultPos);
        }
        public static Tween DOAColor(this SpriteRenderer renderer, Color endValue = default, float duration = 1f)
        {
            return renderer.DOColor(endValue, duration).SetTarget(renderer.gameObject);
        }
        public static Tween DOATextTyping(this TextMeshPro textMesh, string text, float duration, bool clearText = false)
        {
            int curIndex = -1;
            if (clearText) textMesh.text = string.Empty;
            return DOVirtual.Int(0, text.Length - 1, duration, v =>
            {
                if (curIndex < v)
                {
                    curIndex++;
                    textMesh.text += text[curIndex];
                }
            });
        }
        public static Tween DOATextPopUp(this TextMeshPro textmesh, float delay, float2 rotZRange = default, Action onComplete = null)
        {
            const float Y_RANGE_MAX = 15 * Global.NORMAL_TO_PIXEL;
            const float Y_RANGE_MIN = -15 * Global.NORMAL_TO_PIXEL;

            bool rotatesToLeft = Random.value > 0.5f;
            float yStart = textmesh.transform.position.y + Y_RANGE_MAX;
            float yEnd = textmesh.transform.position.y + Y_RANGE_MIN;

            textmesh.transform.eulerAngles = Vector3.forward * Random.Range(rotZRange.x, rotZRange.y).InversedIf(rotatesToLeft);

            Sequence sequence = DOTween.Sequence(textmesh);
            sequence.Append(textmesh.transform.DOMoveY(yStart, 0.5f).SetEase(Ease.OutBack));
            sequence.Append(textmesh.DOColor(Utils.clearWhite, 0.5f).SetDelay(delay));
            sequence.Append(textmesh.transform.DOMoveY(yEnd, 0.5f).SetDelay(delay).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                textmesh.gameObject.Destroy();
            });

            return sequence.Play();
        }
        public static Tween DOATextNumberDelta(this TextMeshPro textmesh, int from, int to, float duration, Func<int, string> formatFunc = null)
        {
            if (to == from) return null;
            Color srcColor = textmesh.color;
            formatFunc ??= v => v.ToString();

            if (to > from)
                 textmesh.color = Color.green;
            else textmesh.color = Color.red;

            void OnUpdate(int value) => textmesh.text = formatFunc(value);
            void OnComplete() => textmesh.color = srcColor;

            return DOVirtual.Int(from, to, duration, OnUpdate).OnComplete(OnComplete);
        }
        public static Tween DOAExplosion(this SpriteRenderer renderer, Action onComplete = null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.transform.DOMove(Vector3.zero, 1).SetEase(Ease.OutCubic));
            sequence.AppendInterval(1f);
            sequence.AppendCallback(() =>
            {
                VFX.CreateScreenFlash(Color.white * 0.8f, 2f);
                onComplete?.Invoke();
                renderer.gameObject.Destroy();
            });

            return sequence.Play();
        }
    }
}

namespace DG.Tweening
{
    public static class Extensions
    {
        public static Tweener DOShakePosition2D(this Transform target, float duration, float strength = 1f, int vibrato = 10, float randomness = 90f, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return DOTween.Shake(() => target.localPosition, x => target.localPosition = x,
                duration, strength, vibrato, randomness, ignoreZAxis: true, fadeOut, randomnessMode)
                .SetTarget(target).SetSpecialStartupMode(SpecialStartupMode.SetShake).SetOptions(snapping);
        }
    }
}
