using Cysharp.Threading.Tasks;
using DG.Tweening;
using GreenOne;
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Effects
{
    /// <summary>
    /// Статический класс, представляющий функции для создания визуальных эффектов.
    /// </summary>
    public static class VFX
    {
        static readonly GameObject _sprite;
        static readonly GameObject _text;
        static readonly GameObject _light;

        static VFX()
        {
            _sprite = Resources.Load<GameObject>("Prefabs/FX/Sprite");
            _text = Resources.Load<GameObject>("Prefabs/FX/Text");
            _light = Resources.Load<GameObject>("Prefabs/FX/Light");
        }

        public static SpriteRenderer CreateSquare(Vector2 size, Vector3 pos, float scale = 1)
        {
            var gameObject = GameObject.Instantiate(_sprite, Global.Root);
            var renderer = gameObject.GetComponent<SpriteRenderer>();

            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale * Vector3.one;

            renderer.size = size / Global.NORMAL_SCALE;
            return renderer;
        }
        public static SpriteRenderer CreateSquare(Vector2 size, Transform parent, float scale = 1)
        {
            var renderer = CreateSquare(size, Vector3.zero, scale);
                renderer.transform.SetParent(parent, false);
            return renderer;
        }

        public static SpriteRenderer CreateSprite(Sprite sprite, Vector3 pos, float scale = 1)
        {
            var gameObject = GameObject.Instantiate(_sprite, Global.Root);
            var renderer = gameObject.GetComponent<SpriteRenderer>();

            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale * Vector3.one;

            renderer.sprite = sprite;
            return renderer;
        }
        public static SpriteRenderer CreateSprite(Sprite sprite, Transform parent, float scale = 1)
        {
            var renderer = CreateSprite(sprite, Vector3.zero, scale);
                renderer.transform.SetParent(parent, false);
            return renderer;
        }

        // TODO: create new Glitch Effect
        [Obsolete] public async static UniTask CreateScreenGlitches()
        {
            await UniTask.Delay(500);
            //var gameObject = GameObject.Instantiate(_glitches, Global.Root);
            //var playTime = gameObject.GetComponent<ParticleSystem>().main.duration;

            //await UniTask.Delay((int)(playTime * 1000));
            //gameObject.Destroy();
        }
        public static SpriteRenderer CreateScreenFlash(Color color, float duration)
        {
            var renderer = CreateScreenBG(color);
                renderer.DOFade(0, duration).OnComplete(renderer.gameObject.Destroy);

            return renderer;
        }

        public static SpriteRenderer CreateScreenBG(Color color)
        {
            var size = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height) / Global.NORMAL_TO_PIXEL;
            var sprite = CreateSquare(size, Vector3.zero);
            sprite.color = color;

            return sprite;
        }
        public static SpriteRenderer CreateScreenBG(Color color, Transform parent)
        {
            var renderer = CreateScreenBG(color);
                renderer.transform.SetParent(parent, false);
            return renderer;
        }

        public static TextMeshPro CreateText(string text, Color color, Vector3 pos, float scale = 1)
        {
            var gameObject = GameObject.Instantiate(_text, Global.Root);
            var textmesh = gameObject.GetComponent<TextMeshPro>();

            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale * Vector3.one;

            textmesh.text = text;
            textmesh.color = color;

            return textmesh;
        }
        public static TextMeshPro CreateText(string text, Color color, Transform parent, float scale = 1)
        {
            var textmesh = CreateText(text, color, Vector3.zero, scale);
                textmesh.transform.SetParent(parent, false);
            return textmesh;
        }

        public static Tween CreateTextAsDamage(this Drawer drawer, int damage, bool isHealing, float scale = 1.5f)
        {
            if (drawer == null) return null;
            TextMeshPro textmesh = CreateText(damage.ToString(), isHealing ? Color.green : Color.red, drawer.transform.position, scale);
            return textmesh.DOATextPopUp(delay: 0f, rotZRange: new float2(10, 25));
        }
        public static Tween CreateTextAsSpeech(this Drawer drawer, string text, Color color, float scale = 0.5f)
        {
            if (drawer == null) return null;
            TextMeshPro textmesh = CreateText(text, color, drawer.transform.position + Vector3.up * 100, scale);
            return textmesh.DOATextPopUp(text.Length * 0.1f, rotZRange: new float2(5, 10));
        }

        public static Light2D CreateLight(Color color, Vector3 pos, float scale = 1)
        {
            var gameObject = GameObject.Instantiate(_light, Global.Root);
            var light = gameObject.GetComponent<Light2D>();

            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale * Vector3.one;

            light.color = color;
            return light;
        }
        public static Light2D CreateLight(Color color, Transform parent, float scale = 1)
        {
            var light = CreateLight(color, Vector3.zero, scale);
                light.transform.SetParent(parent, false);
            return light;
        }

        public static Light2D CreateLightFlash(Color color, Vector3 pos)
        {
            var light = CreateLight(color, pos);
            DOVirtual.Float(1.25f, 0, 1.5f, v => light.intensity = v).SetEase(Ease.OutCubic).OnComplete(light.Destroy);
            return light;
        }
        public static Light2D CreateLightFlash(Color color, Transform parent)
        {
            var light = CreateLightFlash(color, Vector3.zero);
                light.transform.SetParent(parent, false);
            return light;
        }
    }
}
