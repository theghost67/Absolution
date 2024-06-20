using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GreenOne
{
    public class QuickToggle : MonoBehaviour, IPointerClickHandler
    {
        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                targetGraphic.canvasRenderer.SetAlpha(System.Convert.ToSingle(isOn));
            }
        }
        [SerializeField] bool isOn;
        [SerializeField] Graphic targetGraphic;
        [Space(20), SerializeField] UnityEvent onClick;

        void Start()
        {
            targetGraphic.canvasRenderer.SetAlpha(System.Convert.ToSingle(isOn));
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            IsOn = !isOn;
            onClick.Invoke();
        }
    }
}