using Game;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace GreenOne
{
    /// <summary>
    /// Класс, представляющий набор настроек, определяющий способы выравнивания дочерних элементов.
    /// </summary>
    public sealed class AlignSettings
    {
        public Vector2 localPos;
        public AlignAnchor anchor;
        public float2 distance;
        public int fixedAxisCount;
        public bool2 fixedAxes;
        public bool2 inversedAxes;

        bool _hasSizeSelector;
        Func<Transform, Vector2> _sizeSelector;

        bool _hasFilter;
        Func<Transform, bool> _filter;

        public AlignSettings(Vector2 localPos, AlignAnchor anchor, float distance, bool fixedAxisIsX, int fixedAxisChildCount) 
                              : this(localPos, anchor, new float2(distance, distance), fixedAxisIsX, fixedAxisChildCount) { }
        public AlignSettings(Vector2 localPos, AlignAnchor anchor, float2 distance, bool fixedAxisIsX, int fixedAxisChildCount)
        {
            this.localPos = localPos;
            this.anchor = anchor;
            this.distance = distance;
            this.fixedAxisCount = fixedAxisChildCount;

            fixedAxes.x = fixedAxisIsX;
            fixedAxes.y = !fixedAxisIsX;
        }

        public void SetSizeSelector(Func<Transform, Vector2> selector)
        {
            _hasSizeSelector = true;
            _sizeSelector = selector;
        }
        public void SetFilter(Func<Transform, bool> filter)
        {
            _hasFilter = true;
            _filter = filter;
        }

        public void ApplyTo(Transform[] children)
        {
            ApplyTo(i => children[i], children.Length);
        }
        public void ApplyTo(Transform transform)
        {
            ApplyTo(i => transform.GetChild(i), transform.childCount);
        }
        public void ApplyTo(Func<int, Transform> childSelector, int childCount)
        {
            if (fixedAxisCount < 0) throw new ArgumentException("Align size cannot be negative.");

            int index = 0;
            if (childCount == 0 || fixedAxisCount == 0) return;

            int fixedSize = Mathf.Clamp(childCount, 0, fixedAxisCount);
            int flexibleSize = childCount / fixedAxisCount + (childCount % fixedAxisCount == 0 ? 0 : 1);

            int xMax = fixedAxes.x ? fixedSize : flexibleSize;
            int yMax = fixedAxes.y ? fixedSize : flexibleSize;

            float xHalfOffset = xMax / 2f - 0.5f;
            float yHalfOffset = yMax / 2f - 0.5f;

            for (int y = 0; y <= yMax - 1; y++)
            {
                for (int x = 0; x <= xMax - 1; x++)
                {
                    Transform child = childSelector(index);
                    if (_hasFilter && !_filter(child))
                    {
                        x--;
                        if (++index >= childCount) return;
                        continue;
                    }

                    Vector2 childSize = _hasSizeSelector ? _sizeSelector(child) : Vector2.zero;
                    float2 unscaledPos = GetPosUnscaled(x, y, xHalfOffset, yHalfOffset);
                    float2 unscaledDistance = new(childSize.x + distance.x, childSize.y + distance.y);
                    float2 scaledPos = GetPosScaled(unscaledPos, unscaledDistance);

                    scaledPos.x = scaledPos.x.InversedIf(inversedAxes.x);
                    scaledPos.y = scaledPos.y.InversedIf(inversedAxes.y);

                    child.transform.localPosition = new Vector3(scaledPos.x, scaledPos.y, child.transform.localPosition.z);
                    if (++index >= childCount) return;
                }
            }
        }

        float2 GetPosUnscaled(int x, int y, float xHalfOffset, float yHalfOffset)
        {
            float unscaledX;
            float unscaledY;

            if (anchor.HasFlag(AlignAnchor.Left))
                 unscaledX = x;
            else if (anchor.HasFlag(AlignAnchor.Right))
                 unscaledX = - x;
            else unscaledX = x - xHalfOffset;

            if (anchor.HasFlag(AlignAnchor.Bottom))
                 unscaledY = y;
            else if (anchor.HasFlag(AlignAnchor.Top))
                 unscaledY = -y;
            else unscaledY = y - yHalfOffset;

            return new float2(unscaledX, unscaledY);
        }
        float2 GetPosScaled(float2 unscaledPos, float2 unscaledDistance)
        {
            unscaledPos.x *= unscaledDistance.x / Global.NORMAL_SCALE;
            unscaledPos.y *= unscaledDistance.y / Global.NORMAL_SCALE;

            return unscaledPos;
        }
    }
}
