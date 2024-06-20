using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game
{
    // create interfaces like ITextDrawer/ISpriteDrawer with IEnumerable<TextMeshPro> Texts? // example
    public static class DrawerExtensions
    {
        public static Tweener DOColor(this Drawer drawer, Color value, float duration)
        {
            Tweener tween = DOVirtual.Color(drawer.Color, value, duration, c => drawer.SetColor(c));
            tween.SetTarget(drawer);
            return tween;
        }
        public static Tweener DOFade(this Drawer drawer, float value, float duration)
        {
            Tweener tween = DOVirtual.Float(drawer.Alpha, value, duration, a => drawer.SetAlpha(a));
            tween.SetTarget(drawer);
            return tween;
        }

        public static Drawer WithHoverTextEvents(this Drawer drawer, TextMeshPro textMesh = null)
        {
            textMesh = textMesh != null ? textMesh : drawer.transform.GetComponent<TextMeshPro>();

            string text = textMesh.text;
            string hoveredText = $"> {text} <";

            drawer.OnMouseEnter += (s, e) => textMesh.text = hoveredText;
            drawer.OnMouseLeave += (s, e) => textMesh.text = text;

            return drawer;
        }
        public static Drawer WithHoverScaleEvents(this Drawer drawer, Transform scaledTransform = null)
        {
            scaledTransform = scaledTransform != null ? scaledTransform : drawer.transform;

            Vector3 defaultScale = scaledTransform.localScale;
            Vector3 modifiedScale = Vector3.one * 1.15f;

            drawer.OnMouseEnter += (s, e) => scaledTransform.localScale = modifiedScale;
            drawer.OnMouseLeave += (s, e) => scaledTransform.localScale = defaultScale;

            return drawer;
        }
    }
}
