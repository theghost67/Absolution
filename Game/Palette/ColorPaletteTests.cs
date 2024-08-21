using GreenOne;
using TMPro;
using UnityEngine;

namespace Game.Palette
{
    public class ColorPaletteTests : MonoBehaviour
    {
        GameObject _gameObject;
        TMP_InputField[] _inputFields;

        void Start()
        {
            const string PATH = "ROOT/PALETTE TESTS";
            _gameObject = GameObject.Find(PATH);
            if (_gameObject == null) Debug.LogWarning($"GameObject for ColorPaletteTests ({PATH}) was not found.");

            _inputFields = _gameObject.GetComponentsInChildren<TMP_InputField>();
            for (int i = 0; i < _inputFields.Length; i++)
            {
                TMP_InputField field = _inputFields[i];
                field.onEndEdit.AddListener(UpdatePalette);
                field.text = "#" + Utils.ColorToHex(ColorPalette.All[i].ColorCur).ToLower();
            }
        }
        void UpdatePalette(string text)
        {
            try
            {
                Color[] colors = new Color[_inputFields.Length];
                for (int i = 0; i < _inputFields.Length; i++)
                    colors[i] = Utils.HexToColor(_inputFields[i].text);

                for (int i = 0; i < colors.Length; i++)
                    ColorPalette.Current = colors;
            }
            catch { }
        }
    }
}
