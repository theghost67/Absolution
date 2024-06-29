using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GreenOne
{
    /// <summary>
    /// –азличные собственно созданные утилиты.
    /// </summary>
    public static class Utils
    {
        #region Variables
        public static readonly Color clearWhite = new(1, 1, 1, 0);
        #endregion

        #region Functions
        public static float RadianToDegrees(float radian) => radian * (180 / Mathf.PI);
        public static float DegreesToRadian(float degrees) => degrees * (Mathf.PI / 180);

        public static string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex.ToUpper(), out Color color))
                return color;
            else throw new FormatException("Incorrect hex data!");
        }

        public static Vector2 MouseToWorldPos(Camera camera)
        {
            if (camera != null)
                return camera.ScreenToWorldPoint(Input.mousePosition);
            else return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        public static Vector3 MouseToWorldPos(Camera camera, float z)
        {
            Vector3 vector;

            if (camera != null)
                vector = camera.ScreenToWorldPoint(Input.mousePosition);
            else vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            vector.z = z;
            return vector;
        }

        public static Color GetStatColor(float value, float defaultValue, bool inverseWhite = false)
        {
            Color color;

            if (value > defaultValue)
                color = Color.green;
            else if (value < defaultValue)
                color = Color.red;
            else color = inverseWhite ? Color.white : Color.black;

            return color;
        }
        #endregion

        #region Extensions
        public static void InsertionSort<T>(this List<T> list, T value) where T : IComparable<T>
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (value.CompareTo(list[i]) <= 0)
                {
                    list.Insert(i, value);
                    return;
                }
            }
            list.Add(value);
        }
        public static void SetVisibleByScale(this GameObject gameObject, bool value)
        {
            if (value)
                 gameObject.transform.localScale = Vector2.one;
            else gameObject.transform.localScale = Vector2.zero;
        }
        public static void SetTriggers(this EventTrigger eventTrigger, params Trigger[] triggers)
        {
            foreach (var trigger in triggers)
            {
                var entry = new EventTrigger.Entry();
                    entry.eventID = trigger.type;
                    entry.callback.AddListener(trigger.action);

                eventTrigger.triggers.Add(entry);
            }
        }

        public static void Deactivate(this GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
        public static void Activate(this GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        public static T Find<T>(this GameObject gameObject, string name) where T : Component
        {
            var result = gameObject.transform.Find(name);
            if (result == null) return null;
            return result.GetComponent<T>();
        }
        public static T Find<T>(this Transform transform, string name) where T : Component
        {
            var result = transform.Find(name);
            if (result == null) return null;
            return result.GetComponent<T>();
        }

        public static Vector2 GetPreferredSize(this TextMeshPro textMesh, string text, bool revert = false)
        {
            string srcText = textMesh.text;
            textMesh.text = text;
            textMesh.ForceMeshUpdate(ignoreActiveState: true);

            if (revert)
                textMesh.text = srcText;
            return textMesh.textBounds.size;
        }

        public static Transform CreateEmptyObject(this Transform transform, string name)
        {
            var gameObject = new GameObject(name);
            var objTransform = gameObject.transform;
                objTransform.SetParent(transform);
                objTransform.localScale = Vector3.one;
                objTransform.position = transform.position;

            return objTransform;
        }
        public static void Destroy(this Component component)
        {
            Object.Destroy(component);
        }
        public static void Destroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
        public static void DestroyImmediate(this Component component)
        {
            Object.DestroyImmediate(component);
        }
        public static void DestroyImmediate(this GameObject gameObject)
        {
            Object.DestroyImmediate(gameObject);
        }

        public static int InversedIf(this int value, bool statement)
        {
            if (statement)
                return -value;
            else return value;
        }
        public static float InversedIf(this float value, bool statement)
        {
            if (statement)
                 return -value;
            else return value;
        }
        public static Vector2 InversedIf(this Vector2 value, bool statement)
        {
            if (statement)
                return -value;
            else return value;
        }
        public static Vector3 InversedIf(this Vector3 value, bool statement)
        {
            if (statement)
                return -value;
            else return value;
        }

        public static T[] AsArray<T>(this T element)
        {
            return new T[] { element };
        }
        public static object[] ToObjectArray<T>(this T[] array)
        {
            object[] objects = new object[array.Length];

            for (int i = 0; i < objects.Length; i++)
                objects[i] = array[i];

            return objects;
        }
        public static object[] ToObjectArray<T>(this ICollection collection)
        {
            int index = 0;
            object[] objects = new object[collection.Count];

            foreach (var element in collection)
            {
                objects[index] = element;
                index++;
            }

            return objects;
        }

        public static int Rounded(this float value)
        {
            return Mathf.RoundToInt(value);
        }
        public static float Rounded(this float value, int digits)
        {
            return (float)Math.Round(value, digits);
        }

        public static int Ceiling(this float value)
        {
            return (int)Math.Ceiling(value);
        }
        public static int Floor(this float value)
        {
            return (int)Math.Floor(value);
        }

        public static int Abs(this int value)
        {
            return Mathf.Abs(value);
        }
        public static float Abs(this float value)
        {
            return Mathf.Abs(value);
        }

        public static int SelectMax(this int value, int other)
        {
            return value > other ? value : other;
        }
        public static int SelectMin(this int value, int other)
        {
            return value < other ? value : other;
        }
        public static float SelectMax(this float value, float other)
        {
            return value > other ? value : other;
        }
        public static float SelectMin(this float value, float other)
        {
            return value < other ? value : other;
        }

        public static int RangedInt(this int value, int range)
        {
            return Random.Range(value - range, value + range + 1);
        }
        public static int RangedInt(this int value, float range)
        {
            return (int)Random.Range(value - range, value + range + 1);
        }
        public static int RangedInt(this float value, int range)
        {
            return (int)Random.Range(value - range, value + range);
        }
        public static int RangedInt(this float value, float range)
        {
            return (int)Random.Range(value - range, value + range);
        }

        public static float Ranged(this int value, int range)
        {
            return Random.Range((float)value - range, (float)value + range);
        }
        public static float Ranged(this int value, float range)
        {
            return Random.Range(value - range, value + range);
        }
        public static float Ranged(this float value, int range)
        {
            return Random.Range(value - range, value + range + 1);
        }
        public static float Ranged(this float value, float range)
        {
            return Random.Range(value - range, value + range);
        }

        public static int Clamped(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }
        public static int ClampedMin(this int value, int min)
        {
            if (value < min)
                 return min;
            else return value;
        }
        public static int ClampedMax(this int value, int max)
        {
            if (value > max)
                return max;
            else return value;
        }

        public static float Clamped(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }
        public static float ClampedMin(this float value, float min)
        {
            if (value < min)
                return min;
            else return value;
        }
        public static float ClampedMax(this float value, float max)
        {
            if (value > max)
                return max;
            else return value;
        }

        public static string ToSignedNumberString(this int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }
        public static string ToSignedNumberString(this float value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        public static string ToDotString(this float value)
        {
            return value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string ToDotString(this double value)
        {
            return value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        #endregion
    }
}

